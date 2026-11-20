PRAGMA foreign_keys = ON;

INSERT INTO Employee (Id, EmployeeNo, FirstName, LastName, Email, Phone, DepartmentId, JobTitle, HireDate, TerminationDate, Salary, IsActive, CreatedAt, ModifiedAt) VALUES
    (1, 'E-1001', 'Laura', 'Becker', 'laura.becker@brightwave.example', '+49 40 5557 1001', 1, 'Managing Director', '2015-03-01', NULL, 120000.00, 1, '2015-03-01 08:30:00', NULL),
    (2, 'E-1002', 'Daniel', 'Hoffmann', 'daniel.hoffmann@brightwave.example', '+49 40 5557 1002', 2, 'Head of Sales', '2017-06-01', NULL, 88000.00, 1, '2017-06-01 08:30:00', NULL),
    (3, 'E-1003', 'Sophie', 'Klein', 'sophie.klein@brightwave.example', '+49 40 5557 1003', 2, 'Senior Sales Manager', '2018-09-15', NULL, 72000.00, 1, '2018-09-15 08:30:00', NULL),
    (4, 'E-1004', 'Marcus', 'Reed', 'marcus.reed@brightwave.example', '+49 40 5557 1004', 2, 'Sales Representative', '2021-02-01', NULL, 54000.00, 1, '2021-02-01 08:30:00', NULL),
    (5, 'E-1005', 'Julia', 'Neumann', 'julia.neumann@brightwave.example', '+49 40 5557 1005', 2, 'Sales Representative', '2022-08-01', NULL, 52000.00, 1, '2022-08-01 08:30:00', NULL),
    (6, 'E-1006', 'Thomas', 'Richter', 'thomas.richter@brightwave.example', '+49 40 5557 1006', 3, 'Head of Finance', '2016-01-04', NULL, 85000.00, 1, '2016-01-04 08:30:00', NULL),
    (7, 'E-1007', 'Elena', 'Vogel', 'elena.vogel@brightwave.example', '+49 40 5557 1007', 3, 'Accountant', '2020-05-01', NULL, 56000.00, 1, '2020-05-01 08:30:00', NULL),
    (8, 'E-1008', 'Paul', 'Schneider', 'paul.schneider@brightwave.example', '+49 40 5557 1008', 4, 'Support Team Lead', '2019-04-01', NULL, 62000.00, 1, '2019-04-01 08:30:00', NULL),
    (9, 'E-1009', 'Nina', 'Fischer', 'nina.fischer@brightwave.example', '+49 40 5557 1009', 4, 'Support Specialist', '2023-01-09', NULL, 46000.00, 1, '2023-01-09 08:30:00', NULL),
    (10, 'E-1010', 'Oliver', 'Krause', 'oliver.krause@brightwave.example', '+49 40 5557 1010', 5, 'Warehouse Manager', '2018-02-01', NULL, 58000.00, 1, '2018-02-01 08:30:00', NULL),
    (11, 'E-1011', 'Lena', 'Wolf', 'lena.wolf@brightwave.example', '+49 40 5557 1011', 5, 'Logistics Clerk', '2022-11-01', NULL, 41000.00, 1, '2022-11-01 08:30:00', NULL),
    (12, 'E-1012', 'Jonas', 'Meyer', 'jonas.meyer@brightwave.example', '+49 40 5557 1012', 6, 'System Administrator', '2020-10-01', NULL, 64000.00, 1, '2020-10-01 08:30:00', NULL),
    (13, 'E-1013', 'Katrin', 'Schmitt', 'katrin.schmitt@brightwave.example', '+49 40 5557 1013', 2, 'Sales Representative', '2019-03-01', '2025-06-30', 50000.00, 0, '2019-03-01 08:30:00', NULL);

UPDATE Department SET ManagerId = 1 WHERE Id = 1;
UPDATE Department SET ManagerId = 2 WHERE Id = 2;
UPDATE Department SET ManagerId = 6 WHERE Id = 3;
UPDATE Department SET ManagerId = 8 WHERE Id = 4;
UPDATE Department SET ManagerId = 10 WHERE Id = 5;
UPDATE Department SET ManagerId = 12 WHERE Id = 6;

INSERT INTO Customer (Id, CustomerNo, CompanyName, VatId, Industry, Website, Email, Phone, PaymentTermId, CurrencyId, AccountManagerId, CreditLimit, IsActive, IsBlocked, Notes, CreatedAt, ModifiedAt) VALUES
    (1, 'C-1001', 'Harbor Logistics GmbH', 'DE811234567', 'Logistics', 'www.harborlogistics.example', 'office@harborlogistics.example', '+49 40 3098 4400', 3, 1, 2, 50000, 1, 0, NULL, '2022-03-14 10:00:00', NULL),
    (2, 'C-1002', 'Brandt Maschinenbau GmbH', 'DE812345670', 'Manufacturing', 'www.brandt-maschinenbau.example', 'info@brandt-maschinenbau.example', '+49 711 889 2000', 4, 1, 3, 80000, 1, 0, 'Key account. Pays within the cash discount period.', '2021-11-02 10:00:00', NULL),
    (3, 'C-1003', 'Lumen Architects BV', 'NL853456789B01', 'Architecture', 'www.lumen-architects.example', 'hello@lumen-architects.example', '+31 20 555 0143', 3, 1, 4, 25000, 1, 0, NULL, '2023-01-23 10:00:00', NULL),
    (4, 'C-1004', 'Maison Delacroix SAS', 'FR12345678901', 'Retail', 'www.maison-delacroix.example', 'contact@maison-delacroix.example', '+33 4 72 55 01 20', 5, 1, 5, 30000, 1, 0, 'Invoices must quote the PO number.', '2023-06-05 10:00:00', NULL),
    (5, 'C-1005', 'Thames Valley Dental Ltd', 'GB123456789', 'Healthcare', 'www.tvdental.example', 'practice@tvdental.example', '+44 118 496 0123', 3, 3, 4, 20000, 1, 0, NULL, '2024-02-19 10:00:00', NULL),
    (6, 'C-1006', 'Redwood Analytics Inc.', NULL, 'Consulting', 'www.redwoodanalytics.example', 'ops@redwoodanalytics.example', '+1 512 555 0176', 3, 2, 3, 60000, 1, 0, 'US customer, no VAT ID. Export delivery.', '2024-04-08 10:00:00', NULL),
    (7, 'C-1007', 'Alpenblick Hotels AG', 'CHE-123.456.789 MWST', 'Hospitality', 'www.alpenblick-hotels.example', 'operations@alpenblick-hotels.example', '+41 44 555 01 33', 3, 4, 5, 40000, 1, 0, NULL, '2024-07-01 10:00:00', NULL),
    (8, 'C-1008', 'Nordwind Energie GmbH', 'DE823456781', 'Energy', 'www.nordwind-energie.example', 'it@nordwind-energie.example', '+49 511 555 2100', 3, 1, 2, 150000, 1, 0, 'Strategic customer with annual support contract.', '2020-09-14 10:00:00', NULL),
    (9, 'C-1009', 'Kreativ Studio Hamburg', 'DE834567892', 'Media & Design', 'www.kreativstudio-hh.example', 'studio@kreativstudio-hh.example', '+49 40 555 0171', 2, 1, 5, 10000, 1, 0, NULL, '2024-09-30 10:00:00', NULL),
    (10, 'C-1010', 'City of Bremerhaven - Education Office', NULL, 'Public Sector', 'www.bremerhaven-schools.example', 'it-schools@bremerhaven-schools.example', '+49 471 555 0190', 3, 1, 2, 100000, 1, 0, 'Public tender framework agreement 2025/2026.', '2022-10-17 10:00:00', NULL),
    (11, 'C-1011', 'Vandermeer Transport BV', 'NL863456789B01', 'Transport', 'www.vandermeer-transport.example', 'office@vandermeer-transport.example', '+31 10 555 0188', 3, 1, 4, 35000, 1, 1, 'Account blocked: invoice INV-2026-0007 is more than 60 days overdue.', '2023-03-27 10:00:00', NULL),
    (12, 'C-1012', 'Sunrise Bakeries GmbH', 'DE845678903', 'Food & Beverage', 'www.sunrise-bakeries.example', 'office@sunrise-bakeries.example', '+49 231 555 0129', 1, 1, 3, 5000, 0, 0, 'Business closed in 2025. Account kept for reference.', '2022-05-09 10:00:00', NULL),
    (13, 'C-1013', 'Fischer & Partner Tax Advisors', 'DE856789014', 'Tax & Accounting', 'www.fischer-partner.example', 'kanzlei@fischer-partner.example', '+49 69 555 0147', 3, 1, NULL, 15000, 1, 0, 'New customer from website inquiry, no account manager assigned yet.', '2026-10-29 10:00:00', NULL);

INSERT INTO Address (Id, CustomerId, Type, AddressLine2, Street, HouseNo, PostalCode, City, CountryId, IsDefault) VALUES
    (1, 1, 'Main', NULL, 'Hafenstrasse', '12', '20457', 'Hamburg', 1, 1),
    (2, 1, 'Billing', 'Accounts Payable', 'Alsterufer', '5', '20354', 'Hamburg', 1, 1),
    (3, 1, 'Shipping', 'Goods Receiving, Gate 2', 'Speicherstadt Kai', '3', '20457', 'Hamburg', 1, 1),
    (4, 2, 'Main', NULL, 'Industriestrasse', '48', '70565', 'Stuttgart', 1, 1),
    (5, 2, 'Billing', 'Accounting', 'Industriestrasse', '48', '70565', 'Stuttgart', 1, 1),
    (6, 2, 'Shipping', 'Warehouse', 'Werkstrasse', '7', '70469', 'Stuttgart', 1, 1),
    (7, 3, 'Main', NULL, 'Prinsengracht', '220', '1016 HB', 'Amsterdam', 2, 1),
    (8, 4, 'Main', NULL, 'Rue de la Republique', '14', '69002', 'Lyon', 3, 1),
    (9, 4, 'Shipping', 'Stock room', 'Rue Paul Bert', '88', '69003', 'Lyon', 3, 1),
    (10, 5, 'Main', NULL, 'Kings Road', '27', 'RG1 4DP', 'Reading', 12, 1),
    (11, 6, 'Main', NULL, 'Congress Avenue', '1200', '78701', 'Austin', 13, 1),
    (12, 7, 'Main', NULL, 'Bahnhofstrasse', '33', '8001', 'Zurich', 14, 1),
    (13, 7, 'Shipping', 'Reception', 'Seestrasse', '90', '8002', 'Zurich', 14, 1),
    (14, 8, 'Main', NULL, 'Windparkallee', '1', '30159', 'Hannover', 1, 1),
    (15, 8, 'Billing', 'Accounts Payable', 'Windparkallee', '1', '30159', 'Hannover', 1, 1),
    (16, 8, 'Shipping', 'IT Logistics', 'Lagerweg', '20', '30419', 'Hannover', 1, 1),
    (17, 9, 'Main', NULL, 'Schanzenstrasse', '71', '20357', 'Hamburg', 1, 1),
    (18, 10, 'Main', NULL, 'Rathausplatz', '1', '27568', 'Bremerhaven', 1, 1),
    (19, 10, 'Billing', 'Finance Department', 'Rathausplatz', '1', '27568', 'Bremerhaven', 1, 1),
    (20, 10, 'Shipping', 'IT Services Depot', 'Schulstrasse', '18', '27570', 'Bremerhaven', 1, 1),
    (21, 11, 'Main', NULL, 'Maaskade', '90', '3071 KP', 'Rotterdam', 2, 1),
    (22, 12, 'Main', NULL, 'Baeckerstrasse', '9', '44137', 'Dortmund', 1, 1),
    (23, 13, 'Main', NULL, 'Kaiserstrasse', '62', '60311', 'Frankfurt am Main', 1, 1);

INSERT INTO Contact (Id, CustomerId, Salutation, FirstName, LastName, JobTitle, Email, Phone, Mobile, IsPrimary, IsActive, CreatedAt) VALUES
    (1, 1, 'Mr', 'Stefan', 'Lindner', 'Head of IT Procurement', 's.lindner@harborlogistics.example', '+49 40 3098 4410', '+49 171 555 0142', 1, 1, '2022-03-14 11:00:00'),
    (2, 1, 'Ms', 'Petra', 'Scholz', 'Accounts Payable', 'p.scholz@harborlogistics.example', '+49 40 3098 4425', NULL, 0, 1, '2022-03-14 11:00:00'),
    (3, 2, 'Mr', 'Karl', 'Brandt', 'Managing Director', 'k.brandt@brandt-maschinenbau.example', '+49 711 889 2001', '+49 172 555 0188', 1, 1, '2021-11-02 11:00:00'),
    (4, 2, 'Ms', 'Sabine', 'Roth', 'IT Manager', 's.roth@brandt-maschinenbau.example', '+49 711 889 2040', NULL, 0, 1, '2021-11-02 11:00:00'),
    (5, 3, 'Ms', 'Eva', 'de Vries', 'Office Manager', 'e.devries@lumen-architects.example', '+31 20 555 0144', '+31 6 5550 1432', 1, 1, '2023-01-23 11:00:00'),
    (6, 3, 'Mr', 'Joost', 'Bakker', 'Partner Architect', 'j.bakker@lumen-architects.example', '+31 20 555 0145', NULL, 0, 1, '2023-01-23 11:00:00'),
    (7, 4, 'Ms', 'Camille', 'Laurent', 'Operations Manager', 'c.laurent@maison-delacroix.example', '+33 4 72 55 01 21', '+33 6 12 55 01 21', 1, 1, '2023-06-05 11:00:00'),
    (8, 5, 'Dr', 'Helen', 'Carter', 'Practice Manager', 'h.carter@tvdental.example', '+44 118 496 0124', '+44 7700 900124', 1, 1, '2024-02-19 11:00:00'),
    (9, 5, 'Mr', 'James', 'Whitfield', 'IT Contact', 'j.whitfield@tvdental.example', NULL, '+44 7700 900125', 0, 1, '2024-02-19 11:00:00'),
    (10, 6, 'Mr', 'Michael', 'Turner', 'VP Operations', 'm.turner@redwoodanalytics.example', '+1 512 555 0177', '+1 512 555 0178', 1, 1, '2024-04-08 11:00:00'),
    (11, 6, 'Ms', 'Priya', 'Nair', 'Procurement Lead', 'p.nair@redwoodanalytics.example', '+1 512 555 0179', NULL, 0, 1, '2024-04-08 11:00:00'),
    (12, 7, 'Ms', 'Andrea', 'Keller', 'Director of Operations', 'a.keller@alpenblick-hotels.example', '+41 44 555 01 34', '+41 79 555 01 34', 1, 1, '2024-07-01 11:00:00'),
    (13, 8, 'Mr', 'Henrik', 'Lang', 'Head of IT', 'h.lang@nordwind-energie.example', '+49 511 555 2101', '+49 173 555 0121', 1, 1, '2020-09-14 11:00:00'),
    (14, 8, 'Ms', 'Miriam', 'Otto', 'Purchasing', 'm.otto@nordwind-energie.example', '+49 511 555 2120', NULL, 0, 1, '2020-09-14 11:00:00'),
    (15, 8, 'Mr', 'Felix', 'Brandt', 'Chief Financial Officer', 'f.brandt@nordwind-energie.example', '+49 511 555 2100', NULL, 0, 1, '2020-09-14 11:00:00'),
    (16, 9, 'Ms', 'Lisa', 'Hartmann', 'Owner', 'lisa@kreativstudio-hh.example', '+49 40 555 0172', '+49 160 555 0172', 1, 1, '2024-09-30 11:00:00'),
    (17, 10, 'Ms', 'Dorothea', 'Arnold', 'Head of IT Department', 'd.arnold@bremerhaven-schools.example', '+49 471 555 0191', NULL, 1, 1, '2022-10-17 11:00:00'),
    (18, 10, 'Mr', 'Rolf', 'Janssen', 'Procurement', 'r.janssen@bremerhaven-schools.example', '+49 471 555 0195', NULL, 0, 1, '2022-10-17 11:00:00'),
    (19, 11, 'Mr', 'Pieter', 'Vandermeer', 'Owner', 'pieter@vandermeer-transport.example', '+31 10 555 0189', '+31 6 5550 1189', 1, 1, '2023-03-27 11:00:00'),
    (20, 12, 'Mr', 'Heinz', 'Conrad', 'Owner', 'h.conrad@sunrise-bakeries.example', '+49 231 555 0130', NULL, 1, 0, '2022-05-09 11:00:00'),
    (21, 13, 'Ms', 'Birgit', 'Fischer', 'Partner', 'b.fischer@fischer-partner.example', '+49 69 555 0148', '+49 151 555 0148', 1, 1, '2026-10-29 11:00:00');
