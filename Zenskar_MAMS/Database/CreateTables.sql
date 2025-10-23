-- Create Zenskar_DB Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Zenskar_DB')
BEGIN
    CREATE DATABASE Zenskar_DB;
END
GO

USE Zenskar_DB;
GO

-- Create User_Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'User_Table')
BEGIN
    CREATE TABLE User_Table
    (
        User_ID INT IDENTITY(1,1) PRIMARY KEY,
        login_ID NVARCHAR(7) UNIQUE NOT NULL,
        User_Name NVARCHAR(100) NOT NULL,
        Contact_Number NVARCHAR(15) NOT NULL,
        Password NVARCHAR(100) NOT NULL,
        User_Type NVARCHAR(50) CHECK (User_Type IN ('Admin', 'Master', 'Instructor')) NOT NULL,
        Status NVARCHAR(50) CHECK (Status IN ('Inactive', 'Active', 'Pending Approval')) NOT NULL,
        Created_Date DATETIME DEFAULT GETDATE() NOT NULL,
        Approved_By NVARCHAR(100) NULL,
        Approved_Date DATETIME NULL
    );
END
GO

-- Create Student_Data Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Student_Data')
BEGIN
    CREATE TABLE Student_Data
    (
        Student_ID INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        DOB DATE NOT NULL,
        Age INT NOT NULL,
        Gender NVARCHAR(10) NOT NULL,
        Location NVARCHAR(100) NOT NULL,
        Belt NVARCHAR(50) NOT NULL,
        InstructorName NVARCHAR(100) NOT NULL,
        MasterName NVARCHAR(100) NOT NULL,
        ContactNumber NVARCHAR(15) NOT NULL,
        ParentsName NVARCHAR(100) NOT NULL,
        MedicalConditions NVARCHAR(255) NULL,
        LastExamDate DATE NULL,
        Attempts INT DEFAULT 0 NOT NULL,
        DateOfJoining DATE NOT NULL,
        Comments NVARCHAR(255) NULL,
        StudentStatus NVARCHAR(50) CHECK (StudentStatus IN ('Stopped', 'Active')) DEFAULT 'Active' NOT NULL
    );
END
GO

-- Create Requests Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Requests')
BEGIN
    CREATE TABLE Requests
    (
        Request_ID INT IDENTITY(1,1) PRIMARY KEY,
        RequestType NVARCHAR(50) CHECK (RequestType IN ('Update', 'Delete', 'Registration')) NOT NULL,
        RequestedBy NVARCHAR(100) NOT NULL,
        Student_ID INT NULL,
        Status NVARCHAR(50) CHECK (Status IN ('Open', 'Approved', 'Rejected')) DEFAULT 'Open' NOT NULL,
        RejectedReason NVARCHAR(255) NULL,
        RequestedDate DATETIME DEFAULT GETDATE() NOT NULL,
        ApprovedBy NVARCHAR(100) NULL,
        ApprovedDate DATETIME NULL,
        FOREIGN KEY (Student_ID) REFERENCES Student_Data(Student_ID)
    );
END
GO

-- Create default Admin user if it doesn't exist
IF NOT EXISTS (SELECT * FROM User_Table WHERE User_Type = 'Admin')
BEGIN
    INSERT INTO User_Table (login_ID, User_Name, Contact_Number, Password, User_Type, Status, Created_Date, Approved_By, Approved_Date)
    VALUES ('ADMIN001', 'System Administrator', '0000000000', 'admin@123', 'Admin', 'Active', GETDATE(), 'System', GETDATE());
END
GO