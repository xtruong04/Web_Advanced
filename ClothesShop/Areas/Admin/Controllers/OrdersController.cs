using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClothesShop.Data;
using ClothesShop.Models;
using ClothesShop.Areas.Admin.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using ClothesShop.Services; // 👉 Nhớ thêm namespace này

namespace ClothesShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")] // Cả hai đều có quyền vào các mục này
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMomoService _momoService; // 👉 1. Khai báo service

        public OrdersController(ApplicationDbContext context, IMomoService momoService)
        {
            _context = context;
            _momoService = momoService;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    FullName = o.FullName,
                    PhoneNumber = o.PhoneNumber,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    // Thêm .ToString() để sửa lỗi chuyển đổi kiểu dữ liệu
                    OrderStatus = o.orderStatus.ToString(),
                    PaymentStatus = o.paymentStatus.ToString()
                })
                .ToListAsync();

            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderItems) // Lấy danh sách sản phẩm
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            // Chuyển đổi sang ViewModel
            var viewModel = new OrderViewModel
            {
                Id = order.Id,
                FullName = order.FullName,
                PhoneNumber = order.PhoneNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.orderStatus.ToString(),
                PaymentStatus = order.paymentStatus.ToString(),
                // Gộp địa chỉ lại cho đúng định nghĩa ViewModel của bạn
                ShippingAddress = $"{order.Street}, {order.Ward}, {order.District}, {order.City}"
            };

            // Để hiển thị sản phẩm, chúng ta truyền order.OrderItems vào ViewBag hoặc thêm vào ViewModel
            ViewBag.OrderItems = order.OrderItems;

            return View(viewModel);
        }

        // POST: Admin/Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,OrderDate,TotalAmount,FullName,PhoneNumber,Street,Ward,District,City,orderStatus,paymentStatus")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Admin/Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Admin/Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 👉 Thêm MomoTransId vào Bind để tránh bị mất dữ liệu khi update
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,OrderDate,TotalAmount,FullName,PhoneNumber,Street,Ward,District,City,orderStatus,paymentStatus,MomoTransId")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Lấy thông tin đơn hàng gốc từ Database (để lấy TransId cũ và kiểm tra trạng thái cũ)
                    // Dùng AsNoTracking để không bị xung đột khi Update
                    var originalOrder = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

                    if (originalOrder == null) return NotFound();

                    // 👉 LOGIC HOÀN TIỀN MOMO
                    // Kiểm tra: Nếu Admin đang đổi trạng thái thành "Refunded" (Hoàn tiền/Hủy)
                    // (Giả sử trong Enum OrderStatus bạn có trạng thái tên là Cancelled hoặc Refunded)
                    if (order.orderStatus == Order.OrderStatus.Cancelled || order.paymentStatus == Order.PaymentStatus.Refunded) // ⚠️ Thay 'Cancelled' bằng Enum 'Refunded' của bạn nếu có
                    {
                        // Chỉ hoàn tiền nếu đơn hàng ĐÃ THANH TOÁN (Paid) và có MomoTransId
                        if (originalOrder.paymentStatus == Order.PaymentStatus.Paid && !string.IsNullOrEmpty(originalOrder.MomoTransId))
                        {
                            // Gọi Service hoàn tiền
                            var refundResponse = await _momoService.RefundAsync(originalOrder);

                            if (refundResponse.resultCode == 0) // Thành công
                            {
                                order.paymentStatus = Order.PaymentStatus.Refunded; // Cập nhật luôn trạng thái thanh toán
                                TempData["Success"] = "Đã hoàn tiền MoMo thành công!";
                            }
                            else // Thất bại
                            {
                                // Show lỗi ra màn hình và KHÔNG lưu database
                                ModelState.AddModelError("", "Lỗi hoàn tiền MoMo: " + refundResponse.message);
                                return View(order);
                            }
                        }
                    }

                    // 2. Nếu không phải hoàn tiền, hoặc hoàn tiền thành công thì lưu dữ liệu
                    // Đảm bảo MomoTransId không bị mất (do form Edit có thể không gửi lên)
                    order.MomoTransId = originalOrder.MomoTransId;

                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
