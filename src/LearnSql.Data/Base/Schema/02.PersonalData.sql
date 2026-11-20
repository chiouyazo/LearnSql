CREATE TABLE Employee (
    Id                INTEGER PRIMARY KEY AUTOINCREMENT,
    EmployeeNo        NVARCHAR(20)   NOT NULL COLLATE NOCASE,
    FirstName         NVARCHAR(100)  NOT NULL COLLATE NOCASE,
    LastName          NVARCHAR(100)  NOT NULL COLLATE NOCASE,
    Email             NVARCHAR(255)  NOT NULL COLLATE NOCASE,
    Phone             NVARCHAR(50)   NULL,
    DepartmentId      INT            NOT NULL,
    JobTitle          NVARCHAR(100)  NULL,
    HireDate          DATE           NOT NULL,
    TerminationDate   DATE           NULL,
    Salary            DECIMAL(10,2)  NULL CHECK (Salary IS NULL OR Salary >= 0),
    IsActive          BIT            NOT NULL DEFAULT 1 CHECK (IsActive IN (0,1)),
    CreatedAt         DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt        DATETIME       NULL,
    CONSTRAINT UQ_Employee_EmployeeNo UNIQUE (EmployeeNo),
    CONSTRAINT UQ_Employee_Email UNIQUE (Email),
    CONSTRAINT FK_Employee_Department FOREIGN KEY (DepartmentId) REFERENCES Department (Id),
    CONSTRAINT CK_Employee_Termination CHECK (TerminationDate IS NULL OR TerminationDate >= HireDate)
);
CREATE INDEX IX_Employee_Department ON Employee (DepartmentId);
CREATE INDEX IX_Employee_Name ON Employee (LastName, FirstName);

CREATE TABLE Customer (
    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerNo          NVARCHAR(20)   NOT NULL COLLATE NOCASE,
    CompanyName         NVARCHAR(200)  NOT NULL COLLATE NOCASE,
    VatId               NVARCHAR(20)   NULL COLLATE NOCASE,
    Industry            NVARCHAR(100)  NULL,
    Website             NVARCHAR(255)  NULL,
    Email               NVARCHAR(255)  NULL COLLATE NOCASE,
    Phone               NVARCHAR(50)   NULL,
    PaymentTermId       INT            NOT NULL,
    CurrencyId          INT            NOT NULL,
    AccountManagerId    INT            NULL,
    CreditLimit         DECIMAL(18,2)  NOT NULL DEFAULT 0 CHECK (CreditLimit >= 0),
    IsActive            BIT            NOT NULL DEFAULT 1 CHECK (IsActive IN (0,1)),
    IsBlocked           BIT            NOT NULL DEFAULT 0 CHECK (IsBlocked IN (0,1)),
    Notes               NVARCHAR(2000) NULL,
    CreatedAt           DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt          DATETIME       NULL,
    CONSTRAINT UQ_Customer_CustomerNo UNIQUE (CustomerNo),
    CONSTRAINT FK_Customer_PaymentTerm    FOREIGN KEY (PaymentTermId)    REFERENCES PaymentTerm (Id),
    CONSTRAINT FK_Customer_Currency       FOREIGN KEY (CurrencyId)       REFERENCES Currency (Id),
    CONSTRAINT FK_Customer_AccountManager FOREIGN KEY (AccountManagerId) REFERENCES Employee (Id) ON DELETE SET NULL
);
CREATE INDEX IX_Customer_CompanyName    ON Customer (CompanyName);
CREATE INDEX IX_Customer_AccountManager ON Customer (AccountManagerId);

CREATE TABLE Address (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerId      INT            NOT NULL,
    Type            NVARCHAR(20)   NOT NULL COLLATE NOCASE
    CHECK (Type IN ('Billing','Shipping','Main')),
    AddressLine2    NVARCHAR(100)  NULL,
    Street          NVARCHAR(150)  NOT NULL,
    HouseNo         NVARCHAR(10)   NULL,
    PostalCode      NVARCHAR(10)   NOT NULL,
    City            NVARCHAR(100)  NOT NULL COLLATE NOCASE,
    CountryId       INT            NOT NULL,
    IsDefault       BIT            NOT NULL DEFAULT 0 CHECK (IsDefault IN (0,1)),
    CONSTRAINT FK_Address_Customer FOREIGN KEY (CustomerId) REFERENCES Customer (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Address_Country  FOREIGN KEY (CountryId)  REFERENCES Country (Id)
);
CREATE INDEX IX_Address_Customer ON Address (CustomerId);

CREATE UNIQUE INDEX UX_Address_Default ON Address (CustomerId, Type) WHERE IsDefault = 1;

CREATE TABLE Contact (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerId      INT            NOT NULL,
    Salutation      NVARCHAR(20)   NULL,
    FirstName       NVARCHAR(100)  NOT NULL COLLATE NOCASE,
    LastName        NVARCHAR(100)  NOT NULL COLLATE NOCASE,
    JobTitle        NVARCHAR(100)  NULL,
    Email           NVARCHAR(255)  NULL COLLATE NOCASE,
    Phone           NVARCHAR(50)   NULL,
    Mobile          NVARCHAR(50)   NULL,
    IsPrimary       BIT            NOT NULL DEFAULT 0 CHECK (IsPrimary IN (0,1)),
    IsActive        BIT            NOT NULL DEFAULT 1 CHECK (IsActive IN (0,1)),
    CreatedAt       DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Contact_Customer FOREIGN KEY (CustomerId) REFERENCES Customer (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Contact_Reachable CHECK (Email IS NOT NULL OR Phone IS NOT NULL OR Mobile IS NOT NULL)
);
CREATE INDEX IX_Contact_Customer ON Contact (CustomerId);
CREATE INDEX IX_Contact_Email ON Contact (Email);
CREATE UNIQUE INDEX UX_Contact_Primary ON Contact (CustomerId) WHERE IsPrimary = 1;
