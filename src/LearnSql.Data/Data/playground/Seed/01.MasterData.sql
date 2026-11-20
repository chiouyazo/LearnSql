PRAGMA foreign_keys = ON;

INSERT INTO Country (Id, Iso2, Iso3, Name, IsEU) VALUES
    (1, 'DE', 'DEU', 'Germany', 1),
    (2, 'NL', 'NLD', 'Netherlands', 1),
    (3, 'FR', 'FRA', 'France', 1),
    (4, 'AT', 'AUT', 'Austria', 1),
    (5, 'BE', 'BEL', 'Belgium', 1),
    (6, 'ES', 'ESP', 'Spain', 1),
    (7, 'IT', 'ITA', 'Italy', 1),
    (8, 'PL', 'POL', 'Poland', 1),
    (9, 'DK', 'DNK', 'Denmark', 1),
    (10, 'SE', 'SWE', 'Sweden', 1),
    (11, 'IE', 'IRL', 'Ireland', 1),
    (12, 'GB', 'GBR', 'United Kingdom', 0),
    (13, 'US', 'USA', 'United States', 0),
    (14, 'CH', 'CHE', 'Switzerland', 0);

INSERT INTO Currency (Id, Code, Name, Symbol) VALUES
    (1, 'EUR', 'Euro', '€'),
    (2, 'USD', 'US Dollar', '$'),
    (3, 'GBP', 'Pound Sterling', '£'),
    (4, 'CHF', 'Swiss Franc', 'CHF');

INSERT INTO TaxRate (Id, Description, Percent, ValidFrom, ValidTo) VALUES
    (1, '19% standard rate (Germany)', 19.00, '2007-01-01', NULL),
    (2, '7% reduced rate (Germany)', 7.00, '2007-01-01', NULL),
    (3, '0% reverse charge / export', 0.00, '2007-01-01', NULL),
    (4, '20% VAT (United Kingdom)', 20.00, '2011-01-04', NULL),
    (5, '16% temporary standard rate (Germany)', 16.00, '2020-07-01', '2020-12-31'),
    (6, '5% temporary reduced rate (Germany)', 5.00, '2020-07-01', '2020-12-31');

INSERT INTO PaymentTerm (Id, Description, NetDays, DiscountDays, DiscountPercent) VALUES
    (1, 'Due on receipt', 0, NULL, NULL),
    (2, '14 days net', 14, NULL, NULL),
    (3, '30 days net', 30, NULL, NULL),
    (4, '30 days net, 2% discount within 10 days', 30, 10, 2.00),
    (5, '60 days net', 60, NULL, NULL),
    (6, '45 days net', 45, NULL, NULL);

INSERT INTO Department (Id, Code, Name, ManagerId) VALUES
    (1, 'MGT', 'Management', NULL),
    (2, 'SAL', 'Sales', NULL),
    (3, 'FIN', 'Finance & Accounting', NULL),
    (4, 'SUP', 'Customer Support', NULL),
    (5, 'LOG', 'Warehouse & Logistics', NULL),
    (6, 'ITD', 'IT & Infrastructure', NULL);

INSERT INTO ItemGroup (Id, Name, ParentGroupId) VALUES
    (1, 'Hardware', NULL),
    (2, 'Laptops', 1),
    (3, 'Monitors', 1),
    (4, 'Peripherals', 1),
    (5, 'Networking', 1),
    (6, 'Software', NULL),
    (7, 'Licenses', 6),
    (8, 'Services', NULL),
    (9, 'Consulting', 8),
    (10, 'Support & Maintenance', 8);

INSERT INTO Item (Id, ItemNo, Name, Description, ItemGroupId, TaxRateId, Unit, SalesPriceNet, PurchasePrice, IsService, IsActive, CreatedAt) VALUES
    (1, 'HW-LT-000', 'ProBook 14 Business Laptop (previous generation)', '14-inch, Core i5, 8 GB RAM, 256 GB SSD. Discontinued.', 2, 1, 'pcs', 799.00, 610.00, 0, 0, '2020-01-15 09:00:00'),
    (2, 'HW-LT-001', 'ProBook 14 Business Laptop', '14-inch, Core i5, 16 GB RAM, 512 GB SSD, Windows 11 Pro', 2, 1, 'pcs', 949.00, 720.00, 0, 1, '2020-01-15 09:00:00'),
    (3, 'HW-LT-002', 'ProBook 16 Workstation Laptop', '16-inch, Core i7, 32 GB RAM, 1 TB SSD, dedicated graphics', 2, 1, 'pcs', 1549.00, 1190.00, 0, 1, '2020-01-15 09:00:00'),
    (4, 'HW-LT-003', 'UltraSlim 13 Notebook', '13-inch ultraportable, Core i5, 16 GB RAM, 512 GB SSD, 1.1 kg', 2, 1, 'pcs', 1099.00, 840.00, 0, 1, '2020-01-15 09:00:00'),
    (5, 'HW-MN-001', '24-inch Full HD Monitor', 'IPS panel, 1920x1080, height-adjustable stand, HDMI and DisplayPort', 3, 1, 'pcs', 189.00, 121.00, 0, 1, '2020-01-15 09:00:00'),
    (6, 'HW-MN-002', '27-inch 4K USB-C Monitor', 'IPS panel, 3840x2160, USB-C with 65 W power delivery', 3, 1, 'pcs', 449.00, 310.00, 0, 1, '2020-01-15 09:00:00'),
    (7, 'HW-MN-003', '34-inch Ultrawide Curved Monitor', '3440x1440, 144 Hz, USB-C hub', 3, 1, 'pcs', 629.00, 445.00, 0, 1, '2020-01-15 09:00:00'),
    (8, 'HW-PR-001', 'Wireless Keyboard and Mouse Set', 'Quiet keys, 2.4 GHz receiver, German layout', 4, 1, 'pcs', 49.90, 28.00, 0, 1, '2020-01-15 09:00:00'),
    (9, 'HW-PR-002', 'USB-C Docking Station', 'Dual display output, Gigabit Ethernet, 100 W charging', 4, 1, 'pcs', 139.00, 89.00, 0, 1, '2020-01-15 09:00:00'),
    (10, 'HW-PR-003', 'HD Webcam 1080p', 'Autofocus, built-in microphone, privacy shutter', 4, 1, 'pcs', 59.00, 34.00, 0, 1, '2020-01-15 09:00:00'),
    (11, 'HW-PR-004', 'Noise-Cancelling Headset', 'Bluetooth and USB, boom microphone, 30 h battery', 4, 1, 'pcs', 129.00, 78.00, 0, 1, '2020-01-15 09:00:00'),
    (12, 'HW-PR-005', 'Printed Quick Start Guide Set', 'Printed manuals for laptop, monitor and dock', 4, 2, 'pcs', 14.90, 6.00, 0, 1, '2020-01-15 09:00:00'),
    (13, 'HW-NW-001', '24-Port Gigabit Switch', 'Managed layer 2 switch, rack mountable, 4x SFP', 5, 1, 'pcs', 279.00, 190.00, 0, 1, '2020-01-15 09:00:00'),
    (14, 'HW-NW-002', 'Wi-Fi 6 Access Point', 'Dual band, PoE powered, ceiling mount', 5, 1, 'pcs', 169.00, 110.00, 0, 1, '2020-01-15 09:00:00'),
    (15, 'HW-NW-003', 'Cat6 Patch Cable 2 m (10-pack)', 'Shielded, assorted colors', 5, 1, 'pack', 24.50, 12.00, 0, 1, '2020-01-15 09:00:00'),
    (16, 'SW-LI-001', 'Office Suite Business License', 'Per user, 1 year subscription', 7, 1, 'license', 129.00, 95.00, 0, 1, '2020-01-15 09:00:00'),
    (17, 'SW-LI-002', 'Endpoint Security Suite', 'Per device, 1 year subscription', 7, 1, 'license', 39.00, 24.00, 0, 1, '2020-01-15 09:00:00'),
    (18, 'SW-LI-003', 'Backup Software Server License', 'Per server, perpetual, includes 1 year of updates', 7, 1, 'license', 399.00, 280.00, 0, 1, '2020-01-15 09:00:00'),
    (19, 'SW-LI-004', 'Project Management Tool', 'Per user, 1 year subscription', 7, 1, 'license', 119.00, 84.00, 0, 1, '2020-01-15 09:00:00'),
    (20, 'SV-CO-001', 'IT Consulting', 'Hourly rate for consulting and analysis', 9, 1, 'h', 135.00, 80.00, 1, 1, '2020-01-15 09:00:00'),
    (21, 'SV-CO-002', 'Network Planning and Setup', 'Daily rate for planning and installation', 9, 1, 'day', 1080.00, 640.00, 1, 1, '2020-01-15 09:00:00'),
    (22, 'SV-CO-003', 'Employee Training (half day)', 'On-site or remote training session for up to 12 participants', 9, 1, 'session', 650.00, 380.00, 1, 1, '2020-01-15 09:00:00'),
    (23, 'SV-SU-001', 'Standard Support Plan', 'Business hours support, response within 8 hours', 10, 1, 'month', 249.00, 120.00, 1, 1, '2020-01-15 09:00:00'),
    (24, 'SV-SU-002', 'Premium Support Plan 24/7', 'Round-the-clock support, response within 1 hour', 10, 1, 'month', 699.00, 340.00, 1, 1, '2020-01-15 09:00:00'),
    (25, 'SV-SU-003', 'On-site Service Call', 'Technician visit within the Hamburg area', 10, 1, 'call', 189.00, 110.00, 1, 1, '2020-01-15 09:00:00'),
    (26, 'SV-SU-004', 'Hardware Warranty Extension (2 years)', 'Extends the manufacturer warranty by two years', 10, 1, 'pcs', 99.00, 50.00, 1, 1, '2020-01-15 09:00:00');

INSERT INTO Warehouse (Id, Name, City) VALUES
    (1, 'Hamburg Central', 'Hamburg'),
    (2, 'Rotterdam Hub', 'Rotterdam'),
    (3, 'Munich Depot', 'Munich');

INSERT INTO Stock (Id, WarehouseId, ItemId, Quantity, MinQuantity, LastChangedAt) VALUES
    (1, 1, 2, 22, 5, '2026-10-30 16:45:00'),
    (2, 2, 2, 8, 5, '2026-10-30 16:45:00'),
    (3, 1, 3, 24, 6, '2026-10-30 16:45:00'),
    (4, 2, 3, 16, 5, '2026-10-30 16:45:00'),
    (5, 1, 4, 13, 5, '2026-10-30 16:45:00'),
    (6, 2, 4, 7, 5, '2026-10-30 16:45:00'),
    (7, 1, 5, 49, 12, '2026-10-30 16:45:00'),
    (8, 2, 5, 10, 5, '2026-10-30 16:45:00'),
    (9, 3, 5, 19, 5, '2026-10-30 16:45:00'),
    (10, 1, 6, 52, 13, '2026-10-30 16:45:00'),
    (11, 2, 6, 9, 5, '2026-10-30 16:45:00'),
    (12, 3, 6, 23, 5, '2026-10-30 16:45:00'),
    (13, 1, 7, 28, 7, '2026-10-30 16:45:00'),
    (14, 2, 7, 8, 5, '2026-10-30 16:45:00'),
    (15, 1, 8, 51, 12, '2026-10-30 16:45:00'),
    (16, 2, 8, 47, 11, '2026-10-30 16:45:00'),
    (17, 3, 8, 46, 11, '2026-10-30 16:45:00'),
    (18, 1, 9, 48, 12, '2026-10-30 16:45:00'),
    (19, 2, 9, 35, 8, '2026-10-30 16:45:00'),
    (20, 3, 9, 25, 6, '2026-10-30 16:45:00'),
    (21, 1, 10, 110, 27, '2026-10-30 16:45:00'),
    (22, 2, 10, 47, 11, '2026-10-30 16:45:00'),
    (23, 3, 10, 23, 5, '2026-10-30 16:45:00'),
    (24, 1, 11, 145, 36, '2026-10-30 16:45:00'),
    (25, 2, 11, 56, 14, '2026-10-30 16:45:00'),
    (26, 3, 11, 27, 6, '2026-10-30 16:45:00'),
    (27, 1, 12, 68, 17, '2026-10-30 16:45:00'),
    (28, 2, 12, 60, 15, '2026-10-30 16:45:00'),
    (29, 3, 12, 60, 15, '2026-10-30 16:45:00'),
    (30, 1, 13, 47, 11, '2026-10-30 16:45:00'),
    (31, 2, 13, 6, 5, '2026-10-30 16:45:00'),
    (32, 3, 13, 23, 5, '2026-10-30 16:45:00'),
    (33, 1, 14, 47, 11, '2026-10-30 16:45:00'),
    (34, 2, 14, 17, 5, '2026-10-30 16:45:00'),
    (35, 3, 14, 6, 5, '2026-10-30 16:45:00'),
    (36, 1, 15, 24, 6, '2026-10-30 16:45:00'),
    (37, 2, 15, 6, 5, '2026-10-30 16:45:00'),
    (38, 3, 15, 22, 5, '2026-10-30 16:45:00');
