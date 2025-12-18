-- Tables

CREATE TABLE Companies (
    CompanyId INT IDENTITY NOT NULL,
    Name NVARCHAR(60) NOT NULL
);

CREATE TABLE Countries (
    Code NVARCHAR(3) NOT NULL,
    Name NVARCHAR(52) NOT NULL
);

CREATE TABLE Groups (
    GroupId INT IDENTITY NOT NULL,
    Name NVARCHAR(30) NOT NULL
);

CREATE TABLE Genders (
    GenderId INT NOT NULL,
    Name NVARCHAR(6) NOT NULL
);

CREATE TABLE NoteTypes (
    NoteTypeId INT NOT NULL,
    Name NVARCHAR(20) NOT NULL
);

CREATE TABLE Sillage (
    SillageId INT NOT NULL,
    Name NVARCHAR(20) NOT NULL
);

CREATE TABLE Longevity (
    LongevityId INT NOT NULL,
    Name NVARCHAR(20) NOT NULL
);

CREATE TABLE Brands (
    BrandId INT IDENTITY NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    CompanyId INT NOT NULL
);

CREATE TABLE Users (
    UserId INT IDENTITY NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    CreationDate DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Perfumes (
    PerfumeId INT IDENTITY NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(2000),
    LaunchYear INT,
    BrandId INT NOT NULL,
    CountryCode NVARCHAR(3) NOT NULL
);

CREATE TABLE Notes (
    NoteId INT IDENTITY NOT NULL,
    Name NVARCHAR(40) NOT NULL
);

CREATE TABLE PerfumeNote (
    PerfumeId INT NOT NULL,
    NoteId INT NOT NULL,
    NoteTypeId INT NOT NULL
);

CREATE TABLE PerfumeGroup (
    PerfumeId INT NOT NULL,
    GroupId INT NOT NULL
);

CREATE TABLE Reviews (
    ReviewId INT IDENTITY NOT NULL,
    UserId INT NOT NULL,
    PerfumeId INT NOT NULL,
    Rating INT NOT NULL,
    Comment NVARCHAR(2000),
    ReviewDate DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE PerfumePhotos (
    PhotoId INT IDENTITY NOT NULL,
    Path NVARCHAR(500) NOT NULL,
    Description NVARCHAR(255),
    UploadDate DATETIME NOT NULL DEFAULT GETDATE(),
    PerfumeId INT NOT NULL
);

CREATE TABLE NotePhotos (
    PhotoId INT IDENTITY NOT NULL,
    Path NVARCHAR(500) NOT NULL,
    Description NVARCHAR(255),
    UploadDate DATETIME NOT NULL DEFAULT GETDATE(),
    NoteId INT NOT NULL
);

CREATE TABLE ReviewPhotos (
    PhotoId INT IDENTITY NOT NULL,
    Path NVARCHAR(500) NOT NULL,
    Description NVARCHAR(255),
    UploadDate DATETIME NOT NULL DEFAULT GETDATE(),
    ReviewId INT
);

CREATE TABLE Seasons (
    SeasonId INT NOT NULL,
    Name NVARCHAR(6) NOT NULL
);

CREATE TABLE Daytimes (
    DaytimeId INT NOT NULL,
    Name NVARCHAR(5) NOT NULL
);

CREATE TABLE PerfumeSillageVotes (
    PerfumeId INT NOT NULL,
    UserId    INT NOT NULL,
    SillageId INT NOT NULL,
    VoteDate  DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE PerfumeLongevityVotes (
    PerfumeId   INT NOT NULL,
    UserId      INT NOT NULL,
    LongevityId INT NOT NULL,
    VoteDate    DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE PerfumeGenderVotes (
    PerfumeId INT NOT NULL,
    UserId    INT NOT NULL,
    GenderId  INT NOT NULL,
    VoteDate  DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE PerfumeSeasonVotes (
    PerfumeId INT NOT NULL,
    UserId    INT NOT NULL,
    SeasonId  INT NOT NULL,
    VoteDate  DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE PerfumeDaytimeVotes (
    PerfumeId INT NOT NULL,
    UserId    INT NOT NULL,
    DaytimeId INT NOT NULL,
    VoteDate  DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE PerfumeList (
    PerfumeListId  INT IDENTITY NOT NULL,
    UserId         INT NOT NULL,
    Name           NVARCHAR(100) NOT NULL,
    IsSystem       BIT NOT NULL,
    CreationDate   DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE PerfumeListItem (
    PerfumeListItemId INT IDENTITY NOT NULL,
    PerfumeListId     INT NOT NULL,
    PerfumeId         INT NOT NULL,
    CreationDate      DATETIME NOT NULL DEFAULT GETDATE()
);

-- Primary Keys
ALTER TABLE Companies ADD CONSTRAINT PK_Companies PRIMARY KEY (CompanyId);
ALTER TABLE Countries ADD CONSTRAINT PK_Countries PRIMARY KEY (Code);
ALTER TABLE Groups ADD CONSTRAINT PK_Groups PRIMARY KEY (GroupId);
ALTER TABLE Genders ADD CONSTRAINT PK_Genders PRIMARY KEY (GenderId);
ALTER TABLE NoteTypes ADD CONSTRAINT PK_NoteTypes PRIMARY KEY (NoteTypeId);
ALTER TABLE Sillage ADD CONSTRAINT PK_Sillage PRIMARY KEY (SillageId);
ALTER TABLE Longevity ADD CONSTRAINT PK_Longevity PRIMARY KEY (LongevityId);
ALTER TABLE Brands ADD CONSTRAINT PK_Brands PRIMARY KEY (BrandId);
ALTER TABLE Users ADD CONSTRAINT PK_Users PRIMARY KEY (UserId);
ALTER TABLE Perfumes ADD CONSTRAINT PK_Perfumes PRIMARY KEY (PerfumeId);
ALTER TABLE Notes ADD CONSTRAINT PK_Notes PRIMARY KEY (NoteId);
ALTER TABLE PerfumeNote ADD CONSTRAINT PK_PerfumeNote PRIMARY KEY (PerfumeId, NoteId, NoteTypeId);
ALTER TABLE PerfumeGroup ADD CONSTRAINT PK_PerfumeGroup PRIMARY KEY (PerfumeId, GroupId);
ALTER TABLE Reviews ADD CONSTRAINT PK_Reviews PRIMARY KEY (ReviewId);
ALTER TABLE PerfumePhotos ADD CONSTRAINT PK_PerfumePhotos PRIMARY KEY (PhotoId);
ALTER TABLE NotePhotos ADD CONSTRAINT PK_NotePhotos PRIMARY KEY (PhotoId);
ALTER TABLE ReviewPhotos ADD CONSTRAINT PK_ReviewPhotos PRIMARY KEY (PhotoId);
ALTER TABLE Seasons ADD CONSTRAINT PK_Seasons PRIMARY KEY (SeasonId);
ALTER TABLE Daytimes ADD CONSTRAINT PK_Daytimes PRIMARY KEY (DaytimeId);
ALTER TABLE PerfumeSillageVotes ADD CONSTRAINT PK_SillageVotes PRIMARY KEY (PerfumeId, UserId);
ALTER TABLE PerfumeLongevityVotes ADD CONSTRAINT PK_LongevityVotes PRIMARY KEY (PerfumeId, UserId);
ALTER TABLE PerfumeGenderVotes ADD CONSTRAINT PK_GenderVotes PRIMARY KEY (PerfumeId, UserId);
ALTER TABLE PerfumeSeasonVotes ADD CONSTRAINT PK_SeasonVotes PRIMARY KEY (PerfumeId, UserId);
ALTER TABLE PerfumeDaytimeVotes ADD CONSTRAINT PK_DaytimeVotes PRIMARY KEY (PerfumeId, UserId);
ALTER TABLE PerfumeList ADD CONSTRAINT PK_PerfumeList PRIMARY KEY (PerfumeListId);
ALTER TABLE PerfumeListItem ADD CONSTRAINT PK_PerfumeListItem PRIMARY KEY (PerfumeListItemId);

-- Unique Constraints
ALTER TABLE Companies ADD CONSTRAINT UQ_Companies_Name UNIQUE (Name);
ALTER TABLE Countries ADD CONSTRAINT UQ_Countries_Name UNIQUE (Name);
ALTER TABLE Groups ADD CONSTRAINT UQ_Groups_Name UNIQUE (Name);
ALTER TABLE Genders ADD CONSTRAINT UQ_Genders_Name UNIQUE (Name);
ALTER TABLE NoteTypes ADD CONSTRAINT UQ_NoteTypes_Name UNIQUE (Name);
ALTER TABLE Sillage ADD CONSTRAINT UQ_Sillage_Name UNIQUE (Name);
ALTER TABLE Longevity ADD CONSTRAINT UQ_Longevity_Name UNIQUE (Name);
ALTER TABLE Brands ADD CONSTRAINT UQ_Brands_Name UNIQUE (Name);
ALTER TABLE Users ADD CONSTRAINT UQ_Users_Username UNIQUE (Username);
ALTER TABLE Users ADD CONSTRAINT UQ_Users_Email UNIQUE (Email);
ALTER TABLE Notes ADD CONSTRAINT UQ_Notes_Name UNIQUE (Name);
ALTER TABLE Seasons ADD CONSTRAINT UQ_Seasons_Name UNIQUE (Name);
ALTER TABLE Daytimes ADD CONSTRAINT UQ_Daytimes_Name UNIQUE (Name);
ALTER TABLE PerfumePhotos ADD CONSTRAINT UQ_PerfumePhotos_Perfume UNIQUE (PerfumeId);
ALTER TABLE NotePhotos ADD CONSTRAINT UQ_NotePhotos_Note UNIQUE (NoteId);
ALTER TABLE ReviewPhotos ADD CONSTRAINT UQ_ReviewPhotos_Review UNIQUE (ReviewId);
ALTER TABLE PerfumeList ADD CONSTRAINT UQ_PerfumeList_User_Name UNIQUE (UserId, Name);
ALTER TABLE PerfumeListItem ADD CONSTRAINT UQ_PerfumeListItem_List_Perfume UNIQUE (PerfumeListId, PerfumeId);


-- Foreign Keys
ALTER TABLE Brands ADD CONSTRAINT FK_Brands_Company FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId);
ALTER TABLE Perfumes ADD CONSTRAINT FK_Perfumes_Brand FOREIGN KEY (BrandId) REFERENCES Brands(BrandId);
ALTER TABLE Perfumes ADD CONSTRAINT FK_Perfumes_Country FOREIGN KEY (CountryCode) REFERENCES Countries(Code);
ALTER TABLE PerfumeNote ADD CONSTRAINT FK_PerfumeNote_NoteType FOREIGN KEY (NoteTypeId) REFERENCES NoteTypes(NoteTypeId);
ALTER TABLE PerfumeNote ADD CONSTRAINT FK_PerfumeNote_Perfume FOREIGN KEY (PerfumeId) REFERENCES Perfumes(PerfumeId) ON DELETE CASCADE;
ALTER TABLE PerfumeNote ADD CONSTRAINT FK_PerfumeNote_Note FOREIGN KEY (NoteId) REFERENCES Notes(NoteId) ON DELETE CASCADE;
ALTER TABLE PerfumeGroup ADD CONSTRAINT FK_PerfumeGroup_Perfume FOREIGN KEY (PerfumeId) REFERENCES Perfumes(PerfumeId);
ALTER TABLE PerfumeGroup ADD CONSTRAINT FK_PerfumeGroup_Group FOREIGN KEY (GroupId) REFERENCES Groups(GroupId);
ALTER TABLE Reviews ADD CONSTRAINT FK_Reviews_User FOREIGN KEY (UserId) REFERENCES Users(UserId);
ALTER TABLE Reviews ADD CONSTRAINT FK_Reviews_Perfume FOREIGN KEY (PerfumeId) REFERENCES Perfumes(PerfumeId);
ALTER TABLE PerfumePhotos ADD CONSTRAINT FK_PerfumePhotos_Perfume FOREIGN KEY (PerfumeId) REFERENCES Perfumes(PerfumeId) ON DELETE CASCADE;
ALTER TABLE NotePhotos ADD CONSTRAINT FK_NotePhotos_Note FOREIGN KEY (NoteId) REFERENCES Notes(NoteId) ON DELETE CASCADE;
ALTER TABLE ReviewPhotos ADD CONSTRAINT FK_ReviewPhotos_Review FOREIGN KEY (ReviewId) REFERENCES Reviews(ReviewId) ON DELETE SET NULL;
ALTER TABLE PerfumeSillageVotes ADD CONSTRAINT FK_SillageVotes_Perfume FOREIGN KEY (PerfumeId) REFERENCES Perfumes(PerfumeId);
ALTER TABLE PerfumeSillageVotes ADD CONSTRAINT FK_SillageVotes_User FOREIGN KEY (UserId) REFERENCES Users(UserId);
ALTER TABLE PerfumeSillageVotes ADD CONSTRAINT FK_SillageVotes_Sillage FOREIGN KEY (SillageId) REFERENCES Sillage(SillageId);
ALTER TABLE PerfumeLongevityVotes ADD CONSTRAINT FK_LongevityVotes_Perfume FOREIGN KEY (PerfumeId) REFERENCES Perfumes(PerfumeId);
ALTER TABLE PerfumeLongevityVotes ADD CONSTRAINT FK_LongevityVotes_User FOREIGN KEY (UserId) REFERENCES Users(UserId);
ALTER TABLE PerfumeLongevityVotes ADD CONSTRAINT FK_LongevityVotes_Longevity FOREIGN KEY (LongevityId) REFERENCES Longevity(LongevityId);
ALTER TABLE PerfumeGenderVotes ADD CONSTRAINT FK_GenderVotes_Perfume FOREIGN KEY (PerfumeId) REFERENCES Perfumes(PerfumeId);
ALTER TABLE PerfumeGenderVotes ADD CONSTRAINT FK_GenderVotes_User FOREIGN KEY (UserId) REFERENCES Users(UserId);
ALTER TABLE PerfumeGenderVotes ADD CONSTRAINT FK_GenderVotes_Gender FOREIGN KEY (GenderId) REFERENCES Genders(GenderId);
ALTER TABLE PerfumeSeasonVotes ADD CONSTRAINT FK_SeasonVotes_Perfume FOREIGN KEY (PerfumeId) REFERENCES Perfumes(PerfumeId);
ALTER TABLE PerfumeSeasonVotes ADD CONSTRAINT FK_SeasonVotes_User FOREIGN KEY (UserId) REFERENCES Users(UserId);
ALTER TABLE PerfumeSeasonVotes ADD CONSTRAINT FK_SeasonVotes_Season FOREIGN KEY (SeasonId) REFERENCES Seasons(SeasonId);
ALTER TABLE PerfumeDaytimeVotes ADD CONSTRAINT FK_DaytimeVotes_Perfume FOREIGN KEY (PerfumeId) REFERENCES Perfumes(PerfumeId);
ALTER TABLE PerfumeDaytimeVotes ADD CONSTRAINT FK_DaytimeVotes_User FOREIGN KEY (UserId) REFERENCES Users(UserId);
ALTER TABLE PerfumeDaytimeVotes ADD CONSTRAINT FK_DaytimeVotes_Daytime FOREIGN KEY (DaytimeId) REFERENCES Daytimes(DaytimeId);
ALTER TABLE PerfumeList ADD CONSTRAINT FK_PerfumeList_User FOREIGN KEY (UserId) REFERENCES Users(UserId);
ALTER TABLE PerfumeListItem ADD CONSTRAINT FK_PerfumeListItem_List FOREIGN KEY (PerfumeListId) REFERENCES PerfumeList (PerfumeListId) ON DELETE CASCADE;
ALTER TABLE PerfumeListItem ADD CONSTRAINT FK_PerfumeListItem_Perfume FOREIGN KEY (PerfumeId) REFERENCES Perfumes (PerfumeId);

-- Check Constraints
ALTER TABLE Companies ADD CONSTRAINT CHK_Companies_Name CHECK (LEN(Name) > 0);
ALTER TABLE Countries ADD CONSTRAINT CHK_Countries_Code CHECK (LEN(Code) = 3 AND Code = UPPER(Code));
ALTER TABLE Groups ADD CONSTRAINT CHK_Groups_Name CHECK (LEN(Name) > 0);
ALTER TABLE Genders ADD CONSTRAINT CHK_Genders_Name CHECK (Name IN ('Male', 'Female', 'Unisex'));
ALTER TABLE NoteTypes ADD CONSTRAINT CHK_NoteTypes_Name CHECK (Name IN ('Top', 'Middle', 'Base'));
ALTER TABLE Sillage ADD CONSTRAINT CHK_Sillage_Name CHECK (Name IN ('Intimate', 'Moderate', 'Strong', 'Enormous'));
ALTER TABLE Longevity ADD CONSTRAINT CHK_Longevity_Name CHECK (Name IN ('Very Weak', 'Weak', 'Moderate', 'Long Lasting', 'Eternal'));
ALTER TABLE Brands ADD CONSTRAINT CHK_Brands_Name CHECK (LEN(Name) > 0);
ALTER TABLE Users ADD CONSTRAINT CHK_Users_Email CHECK (Email LIKE '%@%.%' AND LEN(Email) >= 5);
ALTER TABLE Users ADD CONSTRAINT CHK_Users_Password CHECK (LEN(Password) >= 8);
ALTER TABLE Perfumes ADD CONSTRAINT CHK_Perfumes_Name CHECK (LEN(Name) > 0);
ALTER TABLE Perfumes ADD CONSTRAINT CHK_Perfumes_LaunchYear CHECK (LaunchYear IS NULL OR (LaunchYear >= 1800 AND LaunchYear <= YEAR(GETDATE())));
ALTER TABLE Notes ADD CONSTRAINT CHK_Notes_Name CHECK (LEN(Name) > 0);
ALTER TABLE Reviews ADD CONSTRAINT CHK_Reviews_Rating CHECK (Rating BETWEEN 1 AND 5);
ALTER TABLE Reviews ADD CONSTRAINT CHK_Reviews_Comment CHECK (Comment IS NULL OR LEN(Comment) <= 2000);
ALTER TABLE PerfumePhotos ADD CONSTRAINT CHK_PerfumePhotos_Path CHECK (Path LIKE '%.jpg' OR Path LIKE '%.png' OR Path LIKE '%.jpeg');
ALTER TABLE NotePhotos ADD CONSTRAINT CHK_NotePhotos_Path CHECK (Path LIKE '%.jpg' OR Path LIKE '%.png' OR Path LIKE '%.jpeg');
ALTER TABLE ReviewPhotos ADD CONSTRAINT CHK_ReviewPhotos_Path CHECK (Path LIKE '%.jpg' OR Path LIKE '%.png' OR Path LIKE '%.jpeg');


-- Indexes
CREATE INDEX IX_Perfumes_Name ON Perfumes(Name);
CREATE INDEX IX_Perfumes_BrandId ON Perfumes(BrandId);
CREATE INDEX IX_Perfumes_CountryCode ON Perfumes(CountryCode);
CREATE INDEX IX_Reviews_PerfumeId ON Reviews(PerfumeId);

CREATE INDEX IX_PerfumeGroup_PerfumeId ON PerfumeGroup(PerfumeId);
CREATE INDEX IX_PerfumeGroup_GroupId ON PerfumeGroup(GroupId);


