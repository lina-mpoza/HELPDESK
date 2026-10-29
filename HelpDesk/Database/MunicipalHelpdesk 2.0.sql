CREATE DATABASE MunicipalHelpdesk;

USE MunicipalHelpdesk;

CREATE TABLE Departments (
    DepID          INT IDENTITY(1,1) NOT NULL,
    DepName        VARCHAR(50)      NULL,
    DepDescription VARCHAR(200)     NULL,
    CONSTRAINT PK_Departments PRIMARY KEY (DepID)
);
select * from Departments;









CREATE TABLE EmploymentStatuses (
    EStatus_ID   INT IDENTITY(1,1) NOT NULL,
    EStatus_Name NVARCHAR(15)      NULL,
    CONSTRAINT PK_EmploymentStatuses PRIMARY KEY (EStatus_ID)
);
select * from EmploymentStatuses;

CREATE TABLE JobLevels (
    Jbl_ID             INT IDENTITY(1,1) NOT NULL,
    Level_Name         NVARCHAR(50)      NULL,
    Level_Description  NVARCHAR(200)     NULL,
    CONSTRAINT PK_JobLevels PRIMARY KEY (Jbl_ID)
);
select * from JobLevels ;


CREATE TABLE JobTitles (
    Job_ID          INT IDENTITY(1,1) NOT NULL,
    Job_Title       NVARCHAR(50)      NULL,
    Job_Description NVARCHAR(200)     NULL,   
    Job_Level_ID    INT               NULL,
    CONSTRAINT PK_JobTitles PRIMARY KEY (Job_ID),
    CONSTRAINT FK_JobTitles_JobLevels
        FOREIGN KEY (Job_Level_ID) REFERENCES JobLevels (Jbl_ID)
);
select * from JobTitles;








CREATE TABLE Municipalities (
    MID            INT IDENTITY(1,1) NOT NULL,
    Municipal_Name NVARCHAR(50)      NULL,
    CONSTRAINT PK_Municipalities PRIMARY KEY (MID)
);
select * from Municipalities;








CREATE TABLE MunicipalitiesDepartments (
    MDID  INT IDENTITY(1,1) NOT NULL,
    MID   INT               NULL,
    DepID INT               NULL,
    CONSTRAINT PK_MunicipalitiesDepartments PRIMARY KEY (MDID),
    CONSTRAINT FK_MunDep_Municipalities
        FOREIGN KEY (MID)   REFERENCES Municipalities (MID),
    CONSTRAINT FK_MunDep_Departments
        FOREIGN KEY (DepID) REFERENCES Departments (DepID)
);
select * from MunicipalitiesDepartments;






CREATE TABLE Statuses (
    Status_ID   INT IDENTITY(1,1) NOT NULL,
    Status_Name VARCHAR(30)      NULL,
    CONSTRAINT PK_Statuses PRIMARY KEY (Status_ID)
);
select * from Statuses;











CREATE TABLE TechnicalIssues (
    Tech_Issue_ID          INT IDENTITY(1,1) NOT NULL,
    Tech_Issue_Description VARCHAR(200)     NULL,
    Tech_Issue_Type        VARCHAR(100)     NULL,
    CONSTRAINT PK_TechnicalIssues PRIMARY KEY (Tech_Issue_ID)
);
select * from TechnicalIssues;






CREATE TABLE TicketSolutions (
    Solution_ID     INT IDENTITY(1,1) NOT NULL,
    Ticket_Solution VARCHAR(200)     NULL,
    Solution_Type   VARCHAR(200)     NULL,
    CONSTRAINT PK_TicketSolutions PRIMARY KEY (Solution_ID)
);
select * from TicketSolutions;








CREATE TABLE Images (
    Image_ID     INT IDENTITY(1,1) NOT NULL,
    Blob_Content VARBINARY(MAX)    NULL,
    File_Name    VARCHAR(50)      NULL,
    Mime_Type    VARCHAR(30)      NULL,
    CONSTRAINT PK_Images PRIMARY KEY (Image_ID)
);
select * from Images;






CREATE TABLE Videos (
    Video_ID     INT IDENTITY(1,1) NOT NULL,
    File_Name    NVARCHAR(50)      NULL,
    Blob_Content VARBINARY(MAX)    NULL,
    Mime_Type    NVARCHAR(30)      NULL,
    CONSTRAINT PK_Videos PRIMARY KEY (Video_ID)
);
select * from videos;





CREATE TABLE Persons (
    Person_ID     INT IDENTITY(1,1) NOT NULL,
    First_Name    VARCHAR(50)      NULL,
    Last_Name     VARCHAR(50)      NULL,
    WorkEmail     VARCHAR(100)     NULL,
    Cell_No       VARCHAR(12)      NULL,
    Username      VARCHAR(50)      NOT NULL,
    Password     VARCHAR(255)     NULL,
    Registered_At DATETIME2(0)      NULL CONSTRAINT DF_Persons_RegisteredAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Persons PRIMARY KEY (Person_ID),
    CONSTRAINT UQ_Persons_Username UNIQUE (Username)
);
select * from persons;


CREATE TABLE Staff (
    Staff_ID   INT IDENTITY(1,1) NOT NULL,
    Person_ID  INT               NULL,
    EStatus_ID INT               NULL,
    MDID       INT               NULL,
    StaffNo    NVARCHAR(15)      NULL,
    Job_ID     INT               NULL,
    CONSTRAINT PK_Staff PRIMARY KEY (Staff_ID),
    CONSTRAINT FK_Staff_Persons
        FOREIGN KEY (Person_ID)  REFERENCES Persons (Person_ID),
    CONSTRAINT FK_Staff_EmpStatus
        FOREIGN KEY (EStatus_ID) REFERENCES EmploymentStatuses (EStatus_ID),
    CONSTRAINT FK_Staff_MunDep
        FOREIGN KEY (MDID)       REFERENCES MunicipalitiesDepartments (MDID),
    CONSTRAINT FK_Staff_JobTitles
        FOREIGN KEY (Job_ID)     REFERENCES JobTitles (Job_ID)
);
select * from staff;






CREATE TABLE GeneralStaff (
    GSID     INT IDENTITY(1,1) NOT NULL,
    Staff_ID INT               NULL,
    CONSTRAINT PK_GeneralStaff PRIMARY KEY (GSID),
    CONSTRAINT FK_GeneralStaff_Staff
        FOREIGN KEY (Staff_ID) REFERENCES Staff (Staff_ID)
);
select * from GeneralStaff;








CREATE TABLE HODs (
    HODID    INT IDENTITY(1,1) NOT NULL,
    Staff_ID INT               NULL,
    DepID    INT               NULL,
    CONSTRAINT PK_HODs PRIMARY KEY (HODID),
    CONSTRAINT FK_HODs_Staff
        FOREIGN KEY (Staff_ID) REFERENCES Staff (Staff_ID),
    CONSTRAINT FK_HODs_Departments
        FOREIGN KEY (DepID)    REFERENCES Departments (DepID)
);
select * from HODs;




CREATE TABLE ITAdmins (
    Admin_ID INT IDENTITY(1,1) NOT NULL,
    Staff_ID INT               NULL,
    CONSTRAINT PK_ITAdmins PRIMARY KEY (Admin_ID),
    CONSTRAINT FK_ITAdmins_Staff
        FOREIGN KEY (Staff_ID) REFERENCES Staff (Staff_ID)
);
select * from ITAdmins;


CREATE TABLE UserSupportStaff (
    USS_ID          INT IDENTITY(1,1) NOT NULL,
    Staff_ID        INT               NULL,
    Office_No       NVARCHAR(15)      NULL,
    Office_Location NVARCHAR(30)      NULL,
    CONSTRAINT PK_UserSupportStaff PRIMARY KEY (USS_ID),
    CONSTRAINT FK_USS_Staff
        FOREIGN KEY (Staff_ID) REFERENCES Staff (Staff_ID)
);
select *  from UserSupportStaff;



CREATE TABLE IssuesTickets (
    Ticket_ID         INT IDENTITY(1,1) NOT NULL,
    Created_Date      DATETIME2(0)      NOT NULL
        CONSTRAINT DF_IssuesTickets_CreatedDate DEFAULT SYSUTCDATETIME(),
    Tech_Issue_ID     INT               NULL,
    Urgency_Level     NVARCHAR(30)      NULL,
    Video_ID          INT               NULL,
    Image_ID          INT               NULL,
    Issue_Description NVARCHAR(200)     NULL,
    Staff_ID          INT               NULL,
    Current_Status_ID INT               NULL,       
    Resolved_At       DATETIME2(0)      NULL,       
    Closed_At         DATETIME2(0)      NULL,      
    CONSTRAINT PK_IssuesTickets PRIMARY KEY (Ticket_ID),
    CONSTRAINT FK_IssuesTickets_Staff
        FOREIGN KEY (Staff_ID)       REFERENCES Staff (Staff_ID),
    CONSTRAINT FK_IssuesTickets_TechIssues
        FOREIGN KEY (Tech_Issue_ID)  REFERENCES TechnicalIssues (Tech_Issue_ID),
    CONSTRAINT FK_IssuesTickets_Images
        FOREIGN KEY (Image_ID)       REFERENCES Images (Image_ID),
    CONSTRAINT FK_IssuesTickets_Videos
        FOREIGN KEY (Video_ID)       REFERENCES Videos (Video_ID),
    CONSTRAINT FK_IssuesTickets_Statuses
        FOREIGN KEY (Current_Status_ID) REFERENCES Statuses (Status_ID)
);
select * from IssuesTickets;

CREATE TABLE TicketAssignment (
    Assignment_ID          INT IDENTITY(1,1) NOT NULL,
    Assignment_Instruction NVARCHAR(200)     NOT NULL,
    Assign_Date            DATETIME2(0)      NOT NULL
        CONSTRAINT DF_TicketAssignment_AssignDate DEFAULT SYSUTCDATETIME(),
    Deadline               DATETIME2(0)      NOT NULL,
    Admin_ID               INT               NOT NULL,
    MID                    INT               NOT NULL,
    USS_ID                 INT               NOT NULL,
    Ticket_ID              INT               NOT NULL,
    CONSTRAINT PK_TicketAssignment PRIMARY KEY (Assignment_ID),
    CONSTRAINT FK_TicketAssignment_ITAdmins
        FOREIGN KEY (Admin_ID)  REFERENCES ITAdmins (Admin_ID),
    CONSTRAINT FK_TicketAssignment_Municipalities
        FOREIGN KEY (MID)       REFERENCES Municipalities (MID),
    CONSTRAINT FK_TicketAssignment_USS
        FOREIGN KEY (USS_ID)    REFERENCES UserSupportStaff (USS_ID),
    CONSTRAINT FK_TicketAssignment_IssuesTickets
        FOREIGN KEY (Ticket_ID) REFERENCES IssuesTickets (Ticket_ID)
);
select * from TicketAssignment;


CREATE TABLE USSTicketsTasks (
    UTT_ID       INT IDENTITY(1,1) NOT NULL,
    Ticket_ID    INT               NULL,
    USS_ID       INT               NULL,
    Status_ID    INT               NULL,
    Note         NVARCHAR(100)     NULL,
    Solution_ID  INT               NULL,
    Date_Solved  DATETIME2(0)      NULL,
    CONSTRAINT PK_USSTicketsTasks PRIMARY KEY (UTT_ID),
    CONSTRAINT FK_USSTicketsTasks_IssuesTickets
        FOREIGN KEY (Ticket_ID)   REFERENCES IssuesTickets (Ticket_ID),
    CONSTRAINT FK_USSTicketsTasks_USS
        FOREIGN KEY (USS_ID)      REFERENCES UserSupportStaff (USS_ID),
    CONSTRAINT FK_USSTicketsTasks_Statuses
        FOREIGN KEY (Status_ID)   REFERENCES Statuses (Status_ID),
    CONSTRAINT FK_USSTicketsTasks_TicketSolutions
        FOREIGN KEY (Solution_ID) REFERENCES TicketSolutions (Solution_ID)
);
select * from USSTicketsTasks;

CREATE TABLE TicketActivity (
    Activity_ID       INT IDENTITY(1,1) NOT NULL,
    Ticket_ID         INT               NOT NULL,
    PerformedBy_Staff INT               NULL,
    Action            NVARCHAR(50)      NOT NULL,   -- 'Created','Assigned','StatusChanged','Commented','Resolved','Closed'
    Old_Value         NVARCHAR(100)     NULL,
    New_Value         NVARCHAR(100)     NULL,
    Note              NVARCHAR(500)     NULL,
    Timestamp         DATETIME2(0)      NOT NULL
        CONSTRAINT DF_TicketActivity_Timestamp DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_TicketActivity PRIMARY KEY (Activity_ID),
    CONSTRAINT FK_TicketActivity_IssuesTickets
        FOREIGN KEY (Ticket_ID)         REFERENCES IssuesTickets (Ticket_ID),
    CONSTRAINT FK_TicketActivity_Staff
        FOREIGN KEY (PerformedBy_Staff) REFERENCES Staff (Staff_ID)
);
select * from TicketActivity;
CREATE INDEX IX_IssuesTickets_Status   ON IssuesTickets (Current_Status_ID);
CREATE INDEX IX_IssuesTickets_Staff    ON IssuesTickets (Staff_ID);
CREATE INDEX IX_IssuesTickets_Created  ON IssuesTickets (Created_Date DESC);

CREATE INDEX IX_TicketAssignment_USS    ON TicketAssignment (USS_ID);
CREATE INDEX IX_TicketAssignment_Ticket ON TicketAssignment (Ticket_ID);
CREATE INDEX IX_TicketAssignment_Admin  ON TicketAssignment (Admin_ID);

CREATE INDEX IX_USSTicketsTasks_Ticket  ON USSTicketsTasks (Ticket_ID);
CREATE INDEX IX_USSTicketsTasks_USS     ON USSTicketsTasks (USS_ID);

CREATE INDEX IX_TicketActivity_Ticket   ON TicketActivity (Ticket_ID, Timestamp DESC);



INSERT INTO Persons
    (First_Name, Last_Name, WorkEmail, Cell_No, Username, Password)
VALUES
    ('Zanele', 'Dlamini', 'zdlamini@ortambodm.gov.za', '0739565978', 'ZANELE_ADMIN_O', 'admin123');

DECLARE @PersonID INT = SCOPE_IDENTITY();

INSERT INTO Staff (Person_ID, StaffNo)
VALUES (@PersonID, 'ORT-ADMIN-001');

DECLARE @StaffID INT = SCOPE_IDENTITY();

INSERT INTO ITAdmins (Staff_ID)
VALUES (@StaffID);




INSERT INTO Municipalities (Municipal_Name)
VALUES ('O.R. Tambo District Municipality');

SELECT * FROM Municipalities;


INSERT INTO Departments (DepName, DepDescription)
VALUES ('Information Technology', 'IT Department');

-- Ensure the link exists
DECLARE @mid INT = (SELECT TOP 1 MID   FROM Municipalities ORDER BY MID);
DECLARE @did INT = (SELECT TOP 1 DepID FROM Departments    ORDER BY DepID);

IF NOT EXISTS (SELECT 1 FROM MunicipalitiesDepartments WHERE MID = @mid AND DepID = @did)
    INSERT INTO MunicipalitiesDepartments (MID, DepID) VALUES (@mid, @did);

DECLARE @mdid INT = (SELECT TOP 1 MDID FROM MunicipalitiesDepartments WHERE MID = @mid AND DepID = @did);


UPDATE Staff
   SET MDID = @mdid
 WHERE Person_ID = (SELECT Person_ID FROM Persons WHERE Username = 'ZANELE_ADMIN_O');

SELECT * FROM MunicipalitiesDepartments;


    INSERT INTO EmploymentStatuses (EStatus_Name) VALUES
        ('Permanent'),
        ('Temporary'),
        ('Contract'),
        ('Intern'),
        ('Probation'),
        ('Seconded'),
        ('Retired');

    INSERT INTO JobLevels (Level_Name, Level_Description) VALUES
        ('Entry',     'Entry level staff'),
        ('Junior',    'Junior staff'),
        ('Middle',    'Middle management'),
        ('Senior',    'Senior management'),
        ('Executive', 'Executive / Director level');


DECLARE @entryLevelId  INT = (SELECT TOP 1 Jbl_ID FROM JobLevels WHERE Level_Name = 'Entry');
DECLARE @juniorLevelId INT = (SELECT TOP 1 Jbl_ID FROM JobLevels WHERE Level_Name = 'Junior');
DECLARE @middleLevelId INT = (SELECT TOP 1 Jbl_ID FROM JobLevels WHERE Level_Name = 'Middle');
DECLARE @seniorLevelId INT = (SELECT TOP 1 Jbl_ID FROM JobLevels WHERE Level_Name = 'Senior');


    INSERT INTO JobTitles (Job_Title, Job_Description, Job_Level_ID) VALUES
        ('IT Support Technician',       'Provides first-line IT support to staff',      @entryLevelId),
        ('Senior IT Technician',        'Handles escalated IT issues',                 @juniorLevelId),
        ('Network Administrator',       'Manages network infrastructure',              @middleLevelId),
        ('Systems Administrator',       'Manages servers and enterprise systems',      @middleLevelId),
        ('Helpdesk Operator',           'Logs and triages incoming tickets',           @entryLevelId),
        ('IT Manager',                  'Manages the IT department',                   @seniorLevelId),
        ('Information Security Officer','Ensures compliance and data protection',      @seniorLevelId),
        ('Software Developer',          'Develops and maintains internal applications',@juniorLevelId);


    INSERT INTO Departments (DepName, DepDescription) VALUES
        ('Human Resources',        'HR Department'),
        ('Finance',                'Finance and Budget'),
        ('Public Works',           'Infrastructure and Public Works'),
        ('Community Services',     'Community and Social Services'),
        ('Corporate Services',     'Corporate and Administrative Services');

SELECT * FROM EmploymentStatuses;
SELECT * FROM JobLevels;
SELECT * FROM JobTitles;
SELECT * FROM Departments;


ALTER TABLE Persons ADD Reset_Otp VARCHAR(6) NULL;
ALTER TABLE Persons ADD Reset_Expiry DATETIME2(0) NULL;

select * from persons;

IF NOT EXISTS (SELECT 1 FROM Statuses WHERE Status_Name = 'Confirmed')
BEGIN
    INSERT INTO Statuses (Status_Name) VALUES ('Confirmed');
    PRINT 'Confirmed status added.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Persons_WorkEmail')
    CREATE UNIQUE INDEX UQ_Persons_WorkEmail
    ON Persons (WorkEmail)
    WHERE WorkEmail IS NOT NULL;

 IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Persons_CellNo')
    CREATE UNIQUE INDEX UQ_Persons_CellNo
    ON Persons (Cell_No)
    WHERE Cell_No IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Staff_StaffNo')
    CREATE UNIQUE INDEX UQ_Staff_StaffNo
    ON Staff (StaffNo)
    WHERE StaffNo IS NOT NULL;

    select * from Persons;
UPDATE Staff SET staffNo = 'ORT-ADMIN-002' WHERE Staff_Id = 10;

SELECT 
    p.Username,
    p.Person_ID,
    s.Staff_ID,
    s.MDID,
    md.MID,
    m.Municipal_Name
FROM Persons p
LEFT JOIN Staff s                    ON s.Person_ID = p.Person_ID
LEFT JOIN MunicipalitiesDepartments md ON md.MDID   = s.MDID
LEFT JOIN Municipalities m           ON m.MID       = md.MID
WHERE p.Username = 'ZANELE_ADMIN_O';

DECLARE @mid INT = (SELECT TOP 1 MID   FROM Municipalities ORDER BY MID);
DECLARE @did INT = (SELECT TOP 1 DepID FROM Departments    ORDER BY DepID);

IF NOT EXISTS (SELECT 1 FROM MunicipalitiesDepartments WHERE MID = @mid AND DepID = @did)
    INSERT INTO MunicipalitiesDepartments (MID, DepID) VALUES (@mid, @did);

DECLARE @mdid INT = (SELECT TOP 1 MDID FROM MunicipalitiesDepartments WHERE MID = @mid AND DepID = @did);


UPDATE Staff
   SET MDID = @mdid
 WHERE Person_ID = (SELECT Person_ID FROM Persons WHERE Username = 'ZANELE_ADMIN_O');

 SELECT Person_ID, Username, WorkEmail, Reset_Otp, Reset_Expiry,
       DATEDIFF(SECOND, SYSUTCDATETIME(), Reset_Expiry) AS SecondsUntilExpiry
FROM Persons
WHERE WorkEmail = 'saleloolwabo99@gmail.com';
update persons
set workemail = 'kubompoza@gmail.com'
where Person_ID = 6;

select * from IssuesTickets;
select * from persons;

