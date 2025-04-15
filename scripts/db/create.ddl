-- Tables

CREATE TABLE Countries (
    Code NVARCHAR(3) NOT NULL,
    Name NVARCHAR(52) NOT NULL
);

CREATE TABLE Groups (
    GroupId INT NOT NULL,
    Name NVARCHAR(30) NOT NULL
);

CREATE TABLE Companies (
    CompanyId INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(60) NOT NULL
);

CREATE TABLE Brands (
    BrandId INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    CompanyId INT NOT NULL
);

CREATE TABLE Notes (
    NoteId INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(40) NOT NULL
);

CREATE TABLE PerfumePhotos (
    PhotoId INT IDENTITY(1,1) NOT NULL,
    Path NVARCHAR(500) NOT NULL,
    Description NVARCHAR(255),
    UploadDate DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Sillage (
    SillageId TINYINT NOT NULL,
    Name NVARCHAR(20) NOT NULL
    -- 1 = Intimate, ..., 4 = Enormous
);

CREATE TABLE Longevity (
    LongevityId TINYINT NOT NULL,
    Name NVARCHAR(20) NOT NULL
    -- 1 = Very Weak, ..., 5 = Eternal
);

CREATE TABLE Perfumes (
    PerfumeId INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(2000),
    LaunchYear SMALLINT,
    BrandId INT,
    CountryCode NVARCHAR(3),
    GroupId INT,
    SillageId TINYINT,
    LongevityId TINYINT
);

CREATE TABLE Reviews (
    ReviewId INT IDENTITY(1,1) NOT NULL,
    PerfumeId INT NOT NULL,
    ReviewDate DATETIME NOT NULL DEFAULT GETDATE(),
    Rating INT NOT NULL,
    Comment NVARCHAR(2000)
);

CREATE TABLE Users (
    UserId INT NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Password NVARCHAR(255) NOT NULL, -- hashed
    CreationDate DATETIME NOT NULL 
);

CREATE TABLE UserNotes (
    UserId INT NOT NULL,
    NoteId INT NOT NULL
);

CREATE TABLE PerfumeNotes (
    PerfumeId INT NOT NULL,
    NoteId INT NOT NULL
);

-- Primary Keys

ALTER TABLE Countries ADD CONSTRAINT PK_Countries PRIMARY KEY (Code);
ALTER TABLE Groups ADD CONSTRAINT PK_Groups PRIMARY KEY (GroupId);
ALTER TABLE Companies ADD CONSTRAINT PK_Companies PRIMARY KEY (CompanyId);
ALTER TABLE Brands ADD CONSTRAINT PK_Brands PRIMARY KEY (BrandId);
ALTER TABLE Notes ADD CONSTRAINT PK_Notes PRIMARY KEY (NoteId);
ALTER TABLE PerfumePhotos ADD CONSTRAINT PK_Photos PRIMARY KEY (PhotoId);
ALTER TABLE Sillage ADD CONSTRAINT PK_Sillage PRIMARY KEY (SillageId);
ALTER TABLE Longevity ADD CONSTRAINT PK_Longevity PRIMARY KEY (LongevityId);
ALTER TABLE Perfumes ADD CONSTRAINT PK_Perfumes PRIMARY KEY (PerfumeId);
ALTER TABLE Reviews ADD CONSTRAINT PK_Reviews PRIMARY KEY (ReviewId);
ALTER TABLE Users ADD CONSTRAINT Users_PK PRIMARY KEY (UserId);
ALTER TABLE UserNotes ADD CONSTRAINT UserNotes_PK PRIMARY KEY (UserId, NoteId);
ALTER TABLE PerfumeNotes ADD CONSTRAINT PerfumeNotes_PK PRIMARY KEY (PerfumeId, NoteId);

-- Foreign Keys

ALTER TABLE Brands
ADD CONSTRAINT FK_Brands_Company FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId);

ALTER TABLE Perfumes
ADD CONSTRAINT FK_Perfumes_Brand FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
    CONSTRAINT FK_Perfumes_Country FOREIGN KEY (CountryCode) REFERENCES Countries(Code),
    CONSTRAINT FK_Perfumes_Group FOREIGN KEY (GroupId) REFERENCES Groups(GroupId),
    CONSTRAINT FK_Perfumes_Sillage FOREIGN KEY (SillageId) REFERENCES Sillage(SillageId),
    CONSTRAINT FK_Perfumes_Longevity FOREIGN KEY (LongevityId) REFERENCES Longevity(LongevityId);

ALTER TABLE Reviews
ADD CONSTRAINT FK_Reviews_Perfume FOREIGN KEY (PerfumeId) REFERENCES Perfumes(PerfumeId);

ALTER TABLE UserNotes 
ADD CONSTRAINT FK_UserNotes_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
	CONSTRAINT FK_UserNotes_Notes FOREIGN KEY (NoteId) REFERENCES Notes(NoteId);

ALTER TABLE PerfumeNotes 
ADD CONSTRAINT FK_PerfumeNotes_Perfumes FOREIGN KEY (PerfumeId) REFERENCES Perfumes(PerfumeId),
	CONSTRAINT FK_PerfumeNotes_Notes FOREIGN KEY (NoteId) REFERENCES Notes(NoteId);
