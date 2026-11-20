PRAGMA foreign_keys = ON;
INSERT INTO Employee (Id, EmployeeNo, FirstName, LastName, Email, Phone, DepartmentId, JobTitle, HireDate, TerminationDate, Salary, IsActive, CreatedAt, ModifiedAt) VALUES
    (14, 'E-1014', 'Tim', 'Ostermann', 'tim.ostermann@brightwave.example', NULL, 2, 'Sales Trainee', '2026-09-01', NULL, NULL, 1, '2026-09-01 08:30:00', NULL),
    (15, 'E-1015', 'Hannah', 'Weber', 'hannah.weber@brightwave.example', NULL, 4, NULL, '2026-10-01', NULL, NULL, 1, '2026-10-01 08:30:00', NULL);
INSERT INTO Customer (Id, CustomerNo, CompanyName, VatId, Industry, Website, Email, Phone, PaymentTermId, CurrencyId, AccountManagerId, CreditLimit, IsActive, IsBlocked, Notes, CreatedAt, ModifiedAt) VALUES
    (14, 'C-1014', 'Polar Sports Equipment AB', 'SE556677889901', 'Retail', 'www.polar-sports.example', 'order@polar-sports.example', NULL, 3, 1, 3, 20000, 1, 0, 'Signed up in the web shop, first order still pending.', '2026-09-14 10:00:00', NULL),
    (15, 'C-1015', 'Greenfield Garden Center Ltd', NULL, 'Retail', NULL, 'office@greenfield-garden.example', '+44 20 555 0199', 3, 3, NULL, 8000, 1, 0, NULL, '2026-10-06 10:00:00', NULL),
    (16, 'C-1016', 'Eifel Brauhaus GmbH', 'DE867890125', 'Food & Beverage', NULL, NULL, '+49 6592 555 0150', 2, 1, 5, 12000, 0, 0, 'Quote only, never ordered. Inactive since request was declined.', '2026-03-02 10:00:00', NULL);
INSERT INTO Address (Id, CustomerId, Type, AddressLine2, Street, HouseNo, PostalCode, City, CountryId, IsDefault) VALUES
    (24, 14, 'Main', NULL, 'Kungsgatan', '44', '411 15', 'Gothenburg', 10, 1),
    (25, 15, 'Main', NULL, 'Garden Lane', '3', 'RG2 7AB', 'Reading', 12, 1),
    (26, 15, 'Shipping', 'Loading dock', 'Trade Park Road', '9', 'RG2 0XY', 'Reading', 12, 1),
    (27, 16, 'Main', NULL, 'Brauereistrasse', '1', '54550', 'Daun', 1, 1);
INSERT INTO Contact (Id, CustomerId, Salutation, FirstName, LastName, JobTitle, Email, Phone, Mobile, IsPrimary, IsActive, CreatedAt) VALUES
    (22, 14, 'Ms', 'Ingrid', 'Larsson', 'Purchasing', 'ingrid.larsson@polar-sports.example', NULL, '+46 70 555 0121', 1, 1, '2026-09-14 11:00:00'),
    (23, 15, 'Mr', 'Oliver', 'Hayes', NULL, NULL, '+44 20 555 0198', NULL, 1, 1, '2026-10-06 11:00:00'),
    (24, 16, NULL, 'Matthias', 'Kunz', 'Owner', NULL, '+49 6592 555 0151', NULL, 1, 0, '2026-03-02 11:00:00');
INSERT INTO Orders (Id, OrderNo, CustomerId, ContactId, EmployeeId, BillingAddressId, ShippingAddressId, CurrencyId, PaymentTermId, Status, OrderDate, RequestedDeliveryDate, CustomerReference, IsContract, ContractStart, ContractEnd, BillingIntervalMonths, IsAutoRenew, Notes, CreatedAt, ModifiedAt) VALUES
    (25, 'ORD-2025-0025', 2, NULL, 3, 5, NULL, 1, 4, 'Completed', '2025-06-10', '2025-06-27', 'BM-2025-044', 0, NULL, NULL, NULL, 0, NULL, '2025-06-10 10:15:00', NULL),
    (26, 'ORD-2025-0026', 8, 13, 2, 15, NULL, 1, 3, 'Completed', '2025-11-18', '2025-12-05', NULL, 0, NULL, NULL, NULL, 0, NULL, '2025-11-18 10:15:00', NULL),
    (27, 'ORD-2025-0027', 1, NULL, 4, 2, 3, 1, 3, 'Completed', '2025-12-09', NULL, NULL, 0, NULL, NULL, NULL, 0, NULL, '2025-12-09 10:15:00', NULL),
    (28, 'ORD-2026-0025', 4, 7, 5, 8, NULL, 1, 5, 'Completed', '2026-02-16', '2026-03-06', 'MD-PO-2301', 0, NULL, NULL, NULL, 0, NULL, '2026-02-16 10:15:00', NULL),
    (29, 'ORD-2026-0026', 3, NULL, 4, 7, 7, 1, 3, 'Completed', '2026-07-14', '2026-07-31', 'LUMEN-0714', 0, NULL, NULL, NULL, 0, NULL, '2026-07-14 10:15:00', NULL),
    (30, 'ORD-2026-0027', 9, NULL, 5, 17, 17, 1, 2, 'Delivered', '2026-10-12', '2026-10-26', NULL, 0, NULL, NULL, NULL, 0, NULL, '2026-10-12 10:15:00', NULL);
INSERT INTO OrderPos (Id, OrderId, PosNo, ItemId, Description, Quantity, Unit, UnitPriceNet, DiscountPercent, TaxRateId) VALUES
    (66, 25, 1, 3, 'ProBook 16 Workstation Laptop', 3, 'pcs', 1549.0, 0, 1),
    (67, 25, 2, 5, '24-inch Full HD Monitor', 10, 'pcs', 189.0, 5, 1),
    (68, 26, 1, 6, '27-inch 4K USB-C Monitor', 12, 'pcs', 449.0, 8, 1),
    (69, 26, 2, 9, 'USB-C Docking Station', 12, 'pcs', 139.0, 8, 1),
    (70, 26, 3, 17, 'Endpoint Security Suite', 12, 'license', 39.0, 0, 1),
    (71, 27, 1, 2, 'ProBook 14 Business Laptop', 8, 'pcs', 949.0, 5, 1),
    (72, 27, 2, 8, 'Wireless Keyboard and Mouse Set', 8, 'pcs', 49.9, 0, 1),
    (73, 28, 1, 4, 'UltraSlim 13 Notebook', 5, 'pcs', 1099.0, 0, 3),
    (74, 28, 2, 10, 'HD Webcam 1080p', 5, 'pcs', 59.0, 0, 3),
    (75, 29, 1, 13, '24-Port Gigabit Switch', 2, 'pcs', 279.0, 0, 3),
    (76, 29, 2, 14, 'Wi-Fi 6 Access Point', 6, 'pcs', 169.0, 0, 3),
    (77, 29, 3, 21, 'Network Planning and Setup', 1, 'day', 1080.0, 0, 3),
    (78, 30, 1, 7, '34-inch Ultrawide Curved Monitor', 2, 'pcs', 629.0, 0, 1),
    (79, 30, 2, 11, 'Noise-Cancelling Headset', 2, 'pcs', 129.0, 0, 1);
INSERT INTO Invoice (Id, InvoiceNo, CustomerId, OrderId, BillingAddressId, CurrencyId, PaymentTermId, CreatedById, InvoiceDate, ServiceDate, DueDate, Status, IsCreditNote, OriginalInvoiceId, CreditNoteReason, TotalNet, TotalTax, Notes, CreatedAt) VALUES
    (27, 'INV-2025-0012', 2, 25, 5, 1, 4, 7, '2025-06-27', '2025-06-27', '2025-07-27', 'Paid', 0, NULL, NULL, 6442.5, 1224.07, NULL, '2025-06-27 09:30:00'),
    (28, 'INV-2025-0013', 8, 26, 15, 1, 3, 7, '2025-12-05', '2025-12-05', '2026-01-04', 'Paid', 0, NULL, NULL, 6959.52, 1322.31, NULL, '2025-12-05 09:30:00'),
    (29, 'INV-2025-0014', 1, 27, 2, 1, 3, 7, '2025-12-23', NULL, '2026-01-22', 'Paid', 0, NULL, NULL, 7611.6, 1446.2, NULL, '2025-12-23 09:30:00'),
    (30, 'INV-2026-0014', 4, 28, 8, 1, 5, 7, '2026-03-06', '2026-03-06', '2026-05-05', 'Paid', 0, NULL, NULL, 5790.0, 0.0, NULL, '2026-03-06 09:30:00'),
    (31, 'INV-2026-0015', 3, 29, 7, 1, 3, 7, '2026-07-31', '2026-07-31', '2026-08-30', 'PartiallyPaid', 0, NULL, NULL, 2652.0, 0.0, NULL, '2026-07-31 09:30:00'),
    (32, 'INV-2026-0016', 9, 30, 17, 1, 2, 7, '2026-10-26', '2026-10-26', '2026-11-09', 'Issued', 0, NULL, NULL, 1516.0, 288.04, NULL, '2026-10-26 09:30:00');
INSERT INTO InvoicePos (Id, InvoiceId, PosNo, OrderPosId, ItemId, Description, Quantity, Unit, UnitPriceNet, DiscountPercent, TaxRateId) VALUES
    (66, 27, 1, 66, 3, 'ProBook 16 Workstation Laptop', 3, 'pcs', 1549, 0, 1),
    (67, 27, 2, 67, 5, '24-inch Full HD Monitor', 10, 'pcs', 189, 5, 1),
    (68, 28, 1, 68, 6, '27-inch 4K USB-C Monitor', 12, 'pcs', 449, 8, 1),
    (69, 28, 2, 69, 9, 'USB-C Docking Station', 12, 'pcs', 139, 8, 1),
    (70, 28, 3, 70, 17, 'Endpoint Security Suite', 12, 'license', 39, 0, 1),
    (71, 29, 1, 71, 2, 'ProBook 14 Business Laptop', 8, 'pcs', 949, 5, 1),
    (72, 29, 2, 72, 8, 'Wireless Keyboard and Mouse Set', 8, 'pcs', 49.9, 0, 1),
    (73, 30, 1, 73, 4, 'UltraSlim 13 Notebook', 5, 'pcs', 1099, 0, 3),
    (74, 30, 2, 74, 10, 'HD Webcam 1080p', 5, 'pcs', 59, 0, 3),
    (75, 31, 1, 75, 13, '24-Port Gigabit Switch', 2, 'pcs', 279, 0, 3),
    (76, 31, 2, 76, 14, 'Wi-Fi 6 Access Point', 6, 'pcs', 169, 0, 3),
    (77, 31, 3, 77, 21, 'Network Planning and Setup', 1, 'day', 1080, 0, 3),
    (78, 32, 1, 78, 7, '34-inch Ultrawide Curved Monitor', 2, 'pcs', 629, 0, 1),
    (79, 32, 2, 79, 11, 'Noise-Cancelling Headset', 2, 'pcs', 129, 0, 1);
INSERT INTO Payment (Id, InvoiceId, PaymentDate, Amount, Method, Reference, RecordedById, RecordedAt) VALUES
    (24, 27, '2025-07-20', 7666.57, 'BankTransfer', 'BM-2025-044', 7, '2025-07-20 15:10:00'),
    (25, 28, '2025-12-28', 8281.83, 'BankTransfer', 'Invoice INV-2025-0013', 7, '2025-12-28 15:10:00'),
    (26, 29, '2026-01-15', 9057.8, 'BankTransfer', 'Invoice INV-2025-0014', 7, '2026-01-15 15:10:00'),
    (27, 30, '2026-03-29', 5790.0, 'BankTransfer', 'MD-PO-2301', 7, '2026-03-29 15:10:00'),
    (28, 31, '2026-08-23', 1326.0, 'BankTransfer', 'LUMEN-0714', 7, '2026-08-23 15:10:00');
