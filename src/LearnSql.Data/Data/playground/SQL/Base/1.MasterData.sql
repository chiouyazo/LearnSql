-- Playground sample data: master data
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
