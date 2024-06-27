-- Check if the database exists, and if it does, drop it
Use master
GO
IF EXISTS (
    SELECT 1
    FROM sys.databases
    WHERE name = 'LibraryManagement'
)
BEGIN
    DROP DATABASE [LibraryManagement];
END

-- Create the new database
CREATE DATABASE [LibraryManagement];
GO

-- Use the newly created database
USE [LibraryManagement];
GO

-- Create the Address table
CREATE TABLE [dbo].[Address] (
    [AddressId] [int] IDENTITY(1,1) NOT NULL,
    [City] [nvarchar](50) NOT NULL,
    [Province] [nvarchar](50) NOT NULL,
    [Country] [nvarchar](50) NOT NULL,
    [POSTALCODE] [nvarchar](10) NOT NULL,
    [AddressLine1] [nvarchar](max) NOT NULL,
    [AddressLine2] [nvarchar](max) NULL,
    PRIMARY KEY CLUSTERED ([AddressId] ASC)
) ON [PRIMARY];
GO
-- Create the Users table
CREATE TABLE [dbo].[Users] (
    [UserId] [int] IDENTITY(1,1) NOT NULL,
    [Username] [nvarchar](20) NULL,
    [FirstName] [nvarchar](50) NOT NULL,
    [LastName] [nvarchar](50) NOT NULL,
    [Dob] [date] NOT NULL,
    [Gender] [nvarchar](50) NOT NULL,
    [Email] [nvarchar](50) NOT NULL,
    [Phone] [nvarchar](50) NOT NULL,
    [Password] [nvarchar](max) NOT NULL,
    [Role] [nchar](10) NULL DEFAULT 'User', -- Default value set to 'User'
    [Timezone] [nvarchar](MAX) NOT NULL DEFAULT 'America/New_York', -- Default value set to 'EST America/New_York Timezone'
	[AddressId] [int] NULL, -- Added AddressId field with ALLOW NULL

    PRIMARY KEY CLUSTERED ([UserId] ASC),
    UNIQUE NONCLUSTERED ([Username] ASC),
    UNIQUE NONCLUSTERED ([Email] ASC),
    CHECK (([Gender] = 'Female' OR [Gender] = 'Male')),
    CHECK (([Role] = 'Owner' OR [Role] = 'User' OR [Role] = 'Admin')),

    -- Foreign Key Constraint
    CONSTRAINT [FK_Users_Address] FOREIGN KEY ([AddressId])
    REFERENCES [dbo].[Address] ([AddressId])
) ON [PRIMARY];
GO




-- Create the AuthenticationRefreshToken table
CREATE TABLE [dbo].[AuthenticationRefreshToken] (
    [userId] [nvarchar](50) NOT NULL,
    [tokenId] [nvarchar](50) NOT NULL,
    [refreshToken] [nvarchar](max) NOT NULL,
    PRIMARY KEY CLUSTERED ([userId] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
GO

-- Create the Category table
CREATE TABLE [dbo].[Category] (
    [CategoryId] [int] IDENTITY(1,1) NOT NULL,
    [name] [nvarchar](max) NOT NULL,
    [imageURL] [nvarchar](max),
    PRIMARY KEY CLUSTERED ([CategoryId] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
GO

-- Create the Books table
CREATE TABLE [dbo].[Books] (
    [BookId] [int] IDENTITY(1,1) NOT NULL,
    [Title] [nvarchar](max) NOT NULL,
    [Author] [nvarchar](max) NOT NULL,
    [TotalQuantity] [int] NOT NULL,
    [AvailableQuantity] [int] NOT NULL,
    [IssuedQuantity] [int] NOT NULL,
    [Price] DECIMAL(10,2) NOT NULL,
    [ImageURL] [nvarchar](max) NOT NULL,
    [CategoryId] [int] NOT NULL,
    PRIMARY KEY CLUSTERED ([BookId] ASC),
    
    -- Foreign Key Constraint
    CONSTRAINT [FK_Books_Category] FOREIGN KEY ([CategoryId])
    REFERENCES [dbo].[Category] ([CategoryId])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
GO

-- Create the BookIssue table
CREATE TABLE [dbo].[BookIssue] (
    [IssueId] [int] IDENTITY(1,1) NOT NULL,
    [BookId] [int] NOT NULL,
    [UserId] [int] NOT NULL,
    [IssueDate] [datetime2] NOT NULL,
    [Days] [int] NOT NULL,
    PRIMARY KEY CLUSTERED ([IssueId] ASC, [BookId] ASC, [UserId] ASC),
    FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books] ([BookId]),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
) ON [PRIMARY];
GO

-- Create the Cart table
CREATE TABLE [dbo].[Cart] (
    [CartId] [int] IDENTITY(1,1) NOT NULL,
    [BookId] [int] NOT NULL,
    [UserId] [int] NOT NULL,
    PRIMARY KEY CLUSTERED ([CartId] ASC, [BookId] ASC, [UserId] ASC),
    FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books] ([BookId]),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
) ON [PRIMARY];
GO

-- Create the SubmitBooksInfo table
CREATE TABLE [dbo].[SubmitBooksInfo] (
    [id] [int] IDENTITY(1,1) NOT NULL,
    [bookId] [int] NOT NULL,
    [bookTitle] [nvarchar](max) NOT NULL,
    [userId] [int] NOT NULL,
    [issueDate] [datetime2] NOT NULL,
    [returnDate] [datetime2] NOT NULL,
    [days] [int] NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
) ON [PRIMARY];
GO

-- Create BillingSummary table
CREATE TABLE BillingSummary (
    billingId INT PRIMARY KEY IDENTITY(1,1),
    userId INT,
    userFirstName NVARCHAR(100) NOT NULL,
    userLastName NVARCHAR(100) NOT NULL,
    userEmail NVARCHAR(100) NOT NULL,
    userPhone NVARCHAR(10) NOT NULL,
    date [datetime2] NOT NULL,
    bookQuantity INT NOT NULL,
    delivery BIT DEFAULT 0,
    addressId INT NOT NULL,
    pickup BIT DEFAULT 0,
    tax DECIMAL(10, 2) NOT NULL,
    totalAmount DECIMAL(10, 2) NOT NULL,
    CONSTRAINT FK_Address FOREIGN KEY (addressId) REFERENCES Address(AddressId)
);
GO
-- Create BillingBooksInfo table
CREATE TABLE BillingBooksInfo (
    BillingBookInfoId INT PRIMARY KEY IDENTITY(1,1),
    bookId INT,
    bookName NVARCHAR(100) NOT NULL,
    rentDays INT NOT NULL,
    estimatedReturnDate DATETIME2 NOT NULL,
    bookAuthor NVARCHAR(50) NOT NULL,
    bookCategory NCHAR(50) NOT NULL,
    bookOriginalPrice DECIMAL(10, 2) NOT NULL,
    bookRentPrice DECIMAL(10, 2) NOT NULL,
    bookImageURL NVARCHAR(MAX) NOT NULL,
    billingId INT NOT NULL,
    CONSTRAINT FK_BillingSummary FOREIGN KEY (billingId) REFERENCES BillingSummary(billingId)
);
GO
USE [LibraryManagement]
GO

SET IDENTITY_INSERT [dbo].[Users] ON 
GO
INSERT [dbo].[Users] ([UserId], [Username], [FirstName], [LastName], [Dob], [Gender], [Email], [Phone], [Password], [Role], [AddressId]) VALUES (1005, N'vp9', N'Vedant', N'Patel', CAST(N'1997-10-19' AS Date), N'Male', N'vedantp9@gmail.com', N'6476274235', N'972rhZml8eMdoOKnWZA48w==;ixuDLjx222q7KCbtDBARzCPTvlvlOWBA9IaHOaC+Vns=', N'Owner     ', NULL)
GO
INSERT [dbo].[Users] ([UserId], [Username], [FirstName], [LastName], [Dob], [Gender], [Email], [Phone], [Password], [Role], [AddressId]) VALUES (1006, N'asurati87', N'Arti', N'Surati', CAST(N'1987-12-03' AS Date), N'Female', N'surti.aarti@gmail.com', N'2267539996', N'972rhZml8eMdoOKnWZA48w==;ixuDLjx222q7KCbtDBARzCPTvlvlOWBA9IaHOaC+Vns=', N'Admin     ', NULL)
GO
INSERT [dbo].[Users] ([UserId], [Username], [FirstName], [LastName], [Dob], [Gender], [Email], [Phone], [Password], [Role], [AddressId]) VALUES (1007, N'yw9', N'Yash', N'Waghela', CAST(N'2000-09-05' AS Date), N'Male', N'yashwaghela1434@gmail.com', N'2265033010', N'fJqIVvyCLZejS0BM6s4dnA==;XKfrpJcr6skvyquLGmYT8o8bFjyHiyuZx7kvHYRkd64=', N'Admin     ', NULL)
GO
INSERT [dbo].[Users] ([UserId], [Username], [FirstName], [LastName], [Dob], [Gender], [Email], [Phone], [Password], [Role], [AddressId]) VALUES (1008, N'ymehta', N'Yashvi', N'Mehta', CAST(N'1998-01-23' AS Date), N'Female', N'yashvi2301@gmail.com', N'6478295324', N'972rhZml8eMdoOKnWZA48w==;ixuDLjx222q7KCbtDBARzCPTvlvlOWBA9IaHOaC+Vns=', N'User      ', NULL)
GO
INSERT [dbo].[Users] ([UserId], [Username], [FirstName], [LastName], [Dob], [Gender], [Email], [Phone], [Password], [Role], [AddressId]) VALUES (1010, N'psoni99', N'Pratik', N'Soni', CAST(N'1999-08-14' AS Date), N'Male', N'pratsss12@gmail.com', N'6475029582', N'pvfNQ//5aV6GrcVllFSXsw==;aEGN4oQDAp0XDmTccGTgWro/dFXAHlwmQ+KNtSNv0F4=', N'User      ', NULL)
GO
INSERT [dbo].[Users] ([UserId], [Username], [FirstName], [LastName], [Dob], [Gender], [Email], [Phone], [Password], [Role], [AddressId]) VALUES (1011, N'vpatel', N'Vedant', N'Patel', CAST(N'1989-06-13' AS Date), N'Male', N'vedantp78@yahoo.com', N'6476274235', N'972rhZml8eMdoOKnWZA48w==;ixuDLjx222q7KCbtDBARzCPTvlvlOWBA9IaHOaC+Vns=', N'User      ', NULL)
GO
INSERT [dbo].[Users] ([UserId], [Username], [FirstName], [LastName], [Dob], [Gender], [Email], [Phone], [Password], [Role], [AddressId]) VALUES (1012, N'dt99', N'Devanshi', N'Thakkar', CAST(N'1999-11-20' AS Date), N'Female', N'devanshit@gmail.com', N'6476274235', N'972rhZml8eMdoOKnWZA48w==;ixuDLjx222q7KCbtDBARzCPTvlvlOWBA9IaHOaC+Vns=', N'User      ', NULL)
GO
INSERT [dbo].[Users] ([UserId], [Username], [FirstName], [LastName], [Dob], [Gender], [Email], [Phone], [Password], [Role], [AddressId]) VALUES (1013, N'ypitroda98', N'Yogiraj', N'Pitroda', CAST(N'1999-11-20' AS Date), N'Female', N'yogipitroda@gmail.com', N'6476274235', N'972rhZml8eMdoOKnWZA48w==;ixuDLjx222q7KCbtDBARzCPTvlvlOWBA9IaHOaC+Vns=', N'User      ', NULL)
GO
INSERT [dbo].[Users] ([UserId], [Username], [FirstName], [LastName], [Dob], [Gender], [Email], [Phone], [Password], [Role], [AddressId]) VALUES (1014, N'apatel91', N'Avdhesh', N'Patel', CAST(N'1991-09-26' AS Date), N'Male', N'avdheshpatel91@gmail.com', N'6476274235', N'972rhZml8eMdoOKnWZA48w==;ixuDLjx222q7KCbtDBARzCPTvlvlOWBA9IaHOaC+Vns=', N'User      ', NULL)
GO

SET IDENTITY_INSERT [dbo].[Users] OFF
GO
SET IDENTITY_INSERT [dbo].[Category] ON 
GO
INSERT [dbo].[Category] ([CategoryId], [name], [imageURL]) VALUES (1, N'Fiction', NULL)
GO
INSERT [dbo].[Category] ([CategoryId], [name], [imageURL]) VALUES (2, N'Non-Fiction', NULL)
GO
INSERT [dbo].[Category] ([CategoryId], [name], [imageURL]) VALUES (3, N'Mystery', NULL)
GO
INSERT [dbo].[Category] ([CategoryId], [name], [imageURL]) VALUES (4, N'Science Fiction', NULL)
GO
INSERT [dbo].[Category] ([CategoryId], [name], [imageURL]) VALUES (5, N'Biography', NULL)
GO
INSERT [dbo].[Category] ([CategoryId], [name], [imageURL]) VALUES (6, N'Novel', NULL)
GO
SET IDENTITY_INSERT [dbo].[Category] OFF
GO
SET IDENTITY_INSERT [dbo].[Books] ON 
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (2, N'To Kill a Mockingbird', N'Harper Lee', 7, 7, 0, CAST(12.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81aY1lxk+9L._AC_UY218_.jpg', 1)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (3, N'1984', N'George Orwell', 7, 7, 0, CAST(8.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/51Rg7oGuhFL._AC_UY218_.jpg', 4)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (4, N'The Da Vinci Code', N'Dan Brown', 7, 7, 0, CAST(15.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/91-zOycXr7L._AC_UY218_.jpg', 3)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (5, N'The Catcher in the Rye', N'J.D. Salinger', 7, 7, 0, CAST(9.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/71nXPGovoTL._SL1500_.jpg', 1)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1004, N'Frontend Development', N'Maya Shavin', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/61Jvmc61xBL._SL1233_.jpg', 2)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1005, N'Real Tigers (Slough House)', N'Mick Herron', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/816pDXN80nS._SL1500_.jpg', 5)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1006, N'Trust (Pulitzer Prize Winner)', N' Hernan Diaz', 7, 7, 0, CAST(8.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/91F6NVlGGjL._SL1500_.jpg', 1)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1007, N'The Fourth Quarter of Your Life: Embracing What Matters Most', N'Matthew Kelly', 7, 7, 0, CAST(8.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/71Cpf9mZn1L._SL1500_.jpg', 1)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1008, N'Horse: A Novel', N'Geraldine Brooks', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81P0W3YcvXL._SL1500_.jpg', 6)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1009, N'Resurrection Walk', N'Michael Connelly', 7, 7, 0, CAST(8.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81C0fz0WS4L._SL1500_.jpg', 1)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1011, N'Dirty Thirty (Stephanie Plum Book 30) ', N'Janet Evanovich', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/71grtUVoFbL._SL1500_.jpg', 1)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1012, N'Friends, Lovers, and the Big Terrible Thing: A Memoir', N'Matthew Perry', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81tdvyI0MeL._SL1500_.jpg', 2)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1013, N'Hello Beautiful (Oprah''s Book Club): A Novel', N'Ann Napolitano', 7, 7, 0, CAST(8.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/91l-m7D1naL._SL1500_.jpg', 6)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1014, N'Elon Musk', N'Walter Isaacson', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81Kaj5++6pL._SL1500_.jpg', 2)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1015, N'Going Infinite: The Rise and Fall of a New Tycoon', N'Michael Lewis', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81LUGiJk5iL._SL1500_.jpg', 2)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1016, N'The Wager: A Tale of Shipwreck, Mutiny and Murder', N' David Grann', 7, 7, 0, CAST(9.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/916BA2dkQpL._SL1500_.jpg', 2)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1017, N'The Fund: Ray Dalio, Bridgewater Associates, and the Unraveling of a Wall Street Legend', N'Rob Copeland', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/91Bei+ZuTcL._SL1500_.jpg', 2)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1018, N'The Secret: A Jack Reacher Novel', N' Lee Child', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81cc7Zt6fvL._SL1500_.jpg', 3)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1019, N'The Edge (6:20 Man Book 2) ', N' David Baldacci', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/91+AC9AYVWL._SL1500_.jpg', 3)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1020, N'The 6:20 Man: A Thriller', N' David Baldacci', 7, 7, 0, CAST(8.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81K94ZwjibL._SL1500_.jpg', 3)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1021, N'Clive Cussler Condor''s Fury (The NUMA Files Book 20)', N'Graham Brown', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/91lRwAIHmDL._SL1500_.jpg', 3)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1022, N'Wool: Book One of the Silo Series', N'Hugh Howey', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81P0TuqTpxL._SL1500_.jpg', 3)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1023, N'Trust (Pulitzer Prize Winner)', N' Hernan Diaz', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81+KUHpN2zL._SL1500_.jpg', 4)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1024, N'Project Hail Mary: A Novel', N' Andy Weir', 7, 7, 0, CAST(9.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81rZH5mC2xL._SL1500_.jpg', 4)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1025, N'Starter Villain', N' John Scalzi', 7, 7, 0, CAST(8.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/71NZSmFYHwL._SL1500_.jpg', 4)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1026, N'The Ark', N' Christopher Coates ', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/91blFDsRgqL._SL1500_.jpg', 4)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1027, N'Out of the Dark', N' David Weber', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/411uGRB4UHL.jpg', 4)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1028, N'Leonardo da Vinci', N' Walter Isaacson', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/91olmCLoVuL._SL1500_.jpg', 5)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1029, N'Gold Dust Woman: The Biography of Stevie Nicks ', N' Stephen Davis', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/91W-tsQ+xqL._SL1500_.jpg', 5)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1030, N'Being Henry: The Fonz . . . and Beyond', N' Henry Winkler', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81f9-YpVUzL._SL1500_.jpg', 5)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1031, N'Amazon Unbound: Jeff Bezos and the Invention of a Global Empire', N' Brad Stone', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/71NDxQtbF8L._SL1500_.jpg', 5)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1032, N'Counting the Cost ', N' Jill Duggar', 7, 7, 0, CAST(9.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/718P7aDQ0TL._SL1500_.jpg', 5)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1033, N'My Name Is Barbra', N' Barbra Streisand', 7, 7, 0, CAST(8.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81V25ee+jsL._SL1500_.jpg', 5)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1034, N'It Starts with Us: A Novel (It Ends with Us Book 2)', N' Colleen Hoover', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/71PNGYHykrL._SL1500_.jpg', 6)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1035, N'Wreck the Halls: A Novel', N' Tessa Bailey', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/810iqurdoHL._SL1500_.jpg', 6)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1036, N'The Intern: A Novel', N'Michele Campbell', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81ga6sHWi+L._SL1500_.jpg', 6)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1037, N'The Secret: A Jack Reacher Novel', N' Lee Child', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81cc7Zt6fvL._SL1500_.jpg', 6)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1038, N'The Nurse''s Secret: A Thrilling Historical Novel of the Dark Side of Gilded Age New York City', N' Amanda Skenandore', 7, 7, 0, CAST(9.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/81N646u31tL._SL1500_.jpg', 6)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1039, N'Ugly Love: A Novel', N'Colleen Hoover', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/71E8VNPC1dL._SL1500_.jpg', 6)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1040, N'Reminders of Him: A Novel', N' Colleen Hoover', 7, 7, 0, CAST(9.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/71rdsaOMvVL._SL1500_.jpg', 6)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1041, N'Woke Up Like This: A Novel', N' Amy Lea', 7, 7, 0, CAST(9.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/91pamnf-5CL._SL1500_.jpg', 6)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1042, N'Dreamworks The Bad Guys: A Very Bad Holiday Novelization ', N'Ms. Kate Howard ', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/61gvFBWZb3L._SL1200_.jpg', 1)
GO
INSERT [dbo].[Books] ([BookId], [Title], [Author], [TotalQuantity], [AvailableQuantity], [IssuedQuantity], [Price], [ImageURL], [CategoryId]) VALUES (1043, N'How to Stop Procrastinating: Powerful Strategies to Overcome Laziness and Multiply', N'Daniel Walter', 7, 7, 0, CAST(10.00 AS Decimal(10, 2)), N'https://m.media-amazon.com/images/I/31+JV5wD3AL._SY445_SX342_.jpg', 1)
GO
SET IDENTITY_INSERT [dbo].[Books] OFF
GO
