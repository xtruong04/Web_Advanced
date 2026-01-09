USE ClothesShop
INSERT INTO [dbo].[Categories] ([Name], [Description]) VALUES 
('Men''s Wear', 'Classic and contemporary clothing for men, including shirts, trousers, and suits.'),
('Women''s Fashion', 'Trendy apparel for women, ranging from elegant dresses to casual tops and skirts.'),
('Outerwear', 'High-quality jackets, coats, and hoodies designed to keep you warm and stylish.'),
('Activewear', 'Performance-driven athletic clothing for gym, running, and outdoor sports.'),
('Footwear', 'A diverse collection of shoes, from formal leather boots to casual sneakers.'),
('Accessories', 'Essential items to complete your look, including belts, hats, scarves, and bags.'),
('Summer Collection', 'Light and breathable fabrics perfect for beach days and hot weather.'),
('Denim & Jeans', 'Durable and fashionable denim wear for all body types and styles.');

-- Giả sử: 1: Men's Wear, 2: Women's Fashion, 3: Outerwear, 4: Activewear, 5: Footwear, 6: Accessories

INSERT INTO [dbo].[Product] ([Name], [Description], [Price], [CategoryId]) VALUES 
-- Men's Wear (CategoryId = 1)
('Slim-Fit Oxford Shirt', 'Classic white cotton shirt, perfect for formal and casual wear.', 45.00, 1),
('Straight-Leg Chinos', 'Premium khaki trousers with a comfortable stretch fit.', 55.50, 1),

-- Women's Fashion (CategoryId = 2)
('Floral Summer Dress', 'Lightweight silk dress with a vibrant floral pattern.', 79.99, 2),
('High-Waisted Skinny Jeans', 'Trendy blue denim with excellent shape retention.', 65.00, 2),

-- Outerwear (CategoryId = 3)
('Water-Resistant Windbreaker', 'Lightweight jacket ideal for breezy and rainy days.', 89.00, 3),
('Classic Wool Overcoat', 'Warm and elegant long coat for the winter season.', 150.00, 3),

-- Activewear (CategoryId = 4)
('Dry-Fit Training Tee', 'Breathable fabric designed for high-intensity workouts.', 29.99, 4),
('Compression Running Leggings', 'Flexible and moisture-wicking leggings for runners.', 49.00, 4),

-- Footwear (CategoryId = 5)
('Urban White Sneakers', 'Minimalist leather sneakers with a cushioned sole.', 110.00, 5),
('Leather Chelsea Boots', 'Handcrafted brown leather boots with elastic side panels.', 135.00, 5),

-- Accessories (CategoryId = 6)
('Classic Leather Belt', 'Genuine leather belt with a polished silver buckle.', 25.00, 6),
('Canvas Laptop Backpack', 'Durable backpack with multiple compartments for daily use.', 59.99, 6);

INSERT INTO [dbo].[ProductImages] ([ImageUrl], [IsThumbnail], [ProductId]) VALUES 
-- Slim-Fit Oxford Shirt (ProductId = 1)
('/images/products/men-oxford-1.jpg', 1, 1),
('/images/products/men-oxford-2.jpg', 0, 1),

-- Straight-Leg Chinos (ProductId = 2)
('/images/products/men-chinos-1.jpg', 1, 2),

-- Floral Summer Dress (ProductId = 3)
('/images/products/women-dress-1.jpg', 1, 3),
('/images/products/women-dress-2.jpg', 0, 3),

-- High-Waisted Skinny Jeans (ProductId = 4)
('/images/products/women-jeans-1.jpg', 1, 4),

-- Water-Resistant Windbreaker (ProductId = 5)
('/images/products/outerwear-wind-1.jpg', 1, 5),

-- Classic Wool Overcoat (ProductId = 6)
('/images/products/outerwear-wool-1.jpg', 1, 6),

-- Dry-Fit Training Tee (ProductId = 7)
('/images/products/sport-tee-1.jpg', 1, 7),

-- Compression Running Leggings (ProductId = 8)
('/images/products/sport-leggings-1.jpg', 1, 8),

-- Urban White Sneakers (ProductId = 9)
('/images/products/shoes-sneaker-1.jpg', 1, 9),
('/images/products/shoes-sneaker-2.jpg', 0, 9),

-- Leather Chelsea Boots (ProductId = 10)
('/images/products/shoes-boots-1.jpg', 1, 10),

-- Classic Leather Belt (ProductId = 11)
('/images/products/access-belt-1.jpg', 1, 11),

-- Canvas Laptop Backpack (ProductId = 12)
('/images/products/access-backpack-1.jpg', 1, 12);

-- Thêm Role Admin
INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());

-- Thêm Role Employee
INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES (NEWID(), 'Employee', 'EMPLOYEE', NEWID());

-- Thêm Role Customer
INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES (NEWID(), 'Customer', 'CUSTOMER', NEWID());

INSERT INTO [dbo].[ProductSizes] ([ProductId], [SizeName], [Inventory])
SELECT 
    p.Id, 
    s.SizeName, 
    ABS(CHECKSUM(NewId())) % 41 + 10 -- Sinh số ngẫu nhiên từ 10 đến 50
FROM [dbo].[Product] p
CROSS JOIN (
    SELECT 'S' AS SizeName UNION ALL
    SELECT 'M' UNION ALL
    SELECT 'L' UNION ALL
    SELECT 'XL'
) s;