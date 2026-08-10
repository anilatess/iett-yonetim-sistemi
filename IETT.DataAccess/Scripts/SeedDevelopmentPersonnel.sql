/*
    Development-only, idempotent sample personnel data.

    This script requires SQLCMD mode and a PasswordHasher<User>-compatible
    hash supplied as the SamplePasswordHash variable. It never contains or
    derives a plaintext password.

    Generate the hash with GenerateDevelopmentPasswordHash.ps1. In SSMS,
    enable Query > SQLCMD Mode and execute this file from an unsaved wrapper:

      :setvar SamplePasswordHash "PASTE_THE_GENERATED_HASH_HERE"
      :r C:\path\to\SeedDevelopmentPersonnel.sql
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

DECLARE @SamplePasswordHash nvarchar(max) =
    LTRIM(RTRIM(N'$(SamplePasswordHash)'));

IF NULLIF(@SamplePasswordHash, N'') IS NULL
    OR @SamplePasswordHash = N'$' + N'(SamplePasswordHash)'
    OR LEFT(@SamplePasswordHash, 6) <> N'AQAAAA'
    OR LEN(@SamplePasswordHash) < 60
    OR @SamplePasswordHash COLLATE Latin1_General_100_BIN2
        LIKE N'%[^A-Za-z0-9+/=]%'
BEGIN
    THROW 51000, N'SamplePasswordHash SQLCMD değişkeni geçerli bir Identity parola hash değeriyle sağlanmalıdır.', 1;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE
        @DriverRoleId int,
        @InspectorRoleId int,
        @IettOperatorId int,
        @OhoOperatorId int,
        @WorkingStatusId int,
        @AnadoluGarageId int,
        @AvrupaGarageId int,
        @IkitelliGarageId int,
        @MatchCount int,
        @Now datetime2 = SYSDATETIME();

    SELECT @DriverRoleId = MIN(Id), @MatchCount = COUNT(*)
    FROM Roles WITH (UPDLOCK, HOLDLOCK)
    WHERE RoleName = N'Driver';
    IF @MatchCount <> 1
        THROW 51001, N'Driver rolü bulunamadı veya birden fazla Driver rolü var.', 1;

    SELECT @InspectorRoleId = MIN(Id), @MatchCount = COUNT(*)
    FROM Roles WITH (UPDLOCK, HOLDLOCK)
    WHERE RoleName = N'Inspector';
    IF @MatchCount <> 1
        THROW 51002, N'Inspector rolü bulunamadı veya birden fazla Inspector rolü var.', 1;

    SELECT @IettOperatorId = MIN(Id), @MatchCount = COUNT(*)
    FROM Operators WITH (UPDLOCK, HOLDLOCK)
    WHERE OperatorName = N'İETT';
    IF @MatchCount <> 1
        THROW 51003, N'İETT operatörü bulunamadı veya birden fazla İETT operatörü var.', 1;

    SELECT @OhoOperatorId = MIN(Id), @MatchCount = COUNT(*)
    FROM Operators WITH (UPDLOCK, HOLDLOCK)
    WHERE OperatorName = N'ÖHO';
    IF @MatchCount <> 1
        THROW 51004, N'ÖHO operatörü bulunamadı veya birden fazla ÖHO operatörü var.', 1;

    SELECT @WorkingStatusId = MIN(Id), @MatchCount = COUNT(*)
    FROM DriverStatuses WITH (UPDLOCK, HOLDLOCK)
    WHERE StatusName = N'Çalışıyor';
    IF @MatchCount <> 1
        THROW 51005, N'Çalışıyor şoför durumu bulunamadı veya birden fazla eşleşme var.', 1;

    SELECT @AnadoluGarageId = MIN(Id), @MatchCount = COUNT(*)
    FROM Garages WITH (UPDLOCK, HOLDLOCK)
    WHERE GarageName = N'Anadolu Garajı';
    IF @MatchCount <> 1
        THROW 51006, N'Anadolu Garajı bulunamadı veya aynı adla birden fazla kayıt var.', 1;

    SELECT @AvrupaGarageId = MIN(Id), @MatchCount = COUNT(*)
    FROM Garages WITH (UPDLOCK, HOLDLOCK)
    WHERE GarageName = N'Avrupa Garajı';
    IF @MatchCount <> 1
        THROW 51007, N'Avrupa Garajı bulunamadı veya aynı adla birden fazla kayıt var.', 1;

    SELECT @IkitelliGarageId = MIN(Id), @MatchCount = COUNT(*)
    FROM Garages WITH (UPDLOCK, HOLDLOCK)
    WHERE GarageName = N'İkitelli Garajı';
    IF @MatchCount > 1
        THROW 51008, N'İkitelli Garajı adıyla birden fazla kayıt var.', 1;

    IF @MatchCount = 0
    BEGIN
        INSERT INTO Garages (GarageName)
        VALUES (N'İkitelli Garajı');
        SET @IkitelliGarageId = CONVERT(int, SCOPE_IDENTITY());
    END;

    DECLARE @Personnel TABLE
    (
        PersonType char(1) NOT NULL,
        FirstName nvarchar(100) NOT NULL,
        LastName nvarchar(100) NOT NULL,
        UserName nvarchar(256) NOT NULL PRIMARY KEY,
        Email nvarchar(256) NOT NULL UNIQUE,
        PhoneNumber nvarchar(32) NOT NULL,
        IdentityNumber nvarchar(32) NOT NULL UNIQUE,
        RoleId int NOT NULL,
        GarageId int NOT NULL,
        OperatorId int NULL,
        DriverStatusId int NULL,
        PersonnelNumber nvarchar(100) NULL,
        HolidayDay nvarchar(32) NULL
    );

    INSERT INTO @Personnel
        (PersonType, FirstName, LastName, UserName, Email, PhoneNumber,
         IdentityNumber, RoleId, GarageId, OperatorId, DriverStatusId,
         PersonnelNumber, HolidayDay)
    VALUES
        ('D', N'Zeynep', N'Arslan', N'zeynep.arslan', N'zeynep.arslan@example.test', N'05000001001', N'99000000001', @DriverRoleId, @AnadoluGarageId, @IettOperatorId, @WorkingStatusId, N'P-1101', N'Salı'),
        ('I', N'Elif', N'Yıldız', N'elif.yildiz', N'elif.yildiz@example.test', N'05000001002', N'99000000002', @InspectorRoleId, @AvrupaGarageId, NULL, NULL, NULL, NULL),
        ('D', N'Can', N'Aydın', N'can.aydin', N'can.aydin@example.test', N'05000001003', N'99000000003', @DriverRoleId, @AvrupaGarageId, @OhoOperatorId, @WorkingStatusId, N'P-2101', N'Çarşamba'),
        ('D', N'Derya', N'Şahin', N'derya.sahin', N'derya.sahin@example.test', N'05000001004', N'99000000004', @DriverRoleId, @AvrupaGarageId, @OhoOperatorId, @WorkingStatusId, N'P-2102', N'Perşembe'),
        ('I', N'Selin', N'Koç', N'selin.koc', N'selin.koc@example.test', N'05000001005', N'99000000005', @InspectorRoleId, @IkitelliGarageId, NULL, NULL, NULL, NULL),
        ('D', N'Burak', N'Çelik', N'burak.celik', N'burak.celik@example.test', N'05000001006', N'99000000006', @DriverRoleId, @IkitelliGarageId, @IettOperatorId, @WorkingStatusId, N'P-3101', N'Cuma'),
        ('D', N'Ece', N'Aksoy', N'ece.aksoy', N'ece.aksoy@example.test', N'05000001007', N'99000000007', @DriverRoleId, @IkitelliGarageId, @IettOperatorId, @WorkingStatusId, N'P-3102', N'Cumartesi'),
        ('D', N'Onur', N'Şen', N'onur.sen', N'onur.sen@example.test', N'05000001008', N'99000000008', @DriverRoleId, @IkitelliGarageId, @IettOperatorId, @WorkingStatusId, N'P-3103', N'Pazar');

    IF EXISTS
    (
        SELECT 1
        FROM @Personnel p
        JOIN Users u WITH (UPDLOCK, HOLDLOCK) ON u.UserName = p.UserName
        WHERE u.RoleId <> p.RoleId
           OR u.FirstName <> p.FirstName
           OR u.LastName <> p.LastName
           OR u.Email <> p.Email
           OR u.PhoneNumber <> p.PhoneNumber
           OR u.IdentityNumber <> p.IdentityNumber
    )
        THROW 51009, N'Mevcut UserName kayıtlarından biri beklenen rol veya temel kimlik bilgileriyle uyuşmuyor.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Personnel p
        JOIN Users u WITH (UPDLOCK, HOLDLOCK) ON u.Email = p.Email
        WHERE u.UserName <> p.UserName
    )
        THROW 51010, N'Örnek email adreslerinden biri farklı bir kullanıcı tarafından kullanılıyor.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Personnel p
        JOIN Users u WITH (UPDLOCK, HOLDLOCK) ON u.IdentityNumber = p.IdentityNumber
        WHERE u.UserName <> p.UserName
    )
        THROW 51011, N'Örnek IdentityNumber değerlerinden biri farklı bir kullanıcı tarafından kullanılıyor.', 1;

    INSERT INTO Users
        (RoleId, FirstName, LastName, IdentityNumber, PhoneNumber, Email,
         UserName, PasswordHash, CreatedDate)
    SELECT p.RoleId, p.FirstName, p.LastName, p.IdentityNumber,
           p.PhoneNumber, p.Email, p.UserName, @SamplePasswordHash, @Now
    FROM @Personnel p
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM Users u WITH (UPDLOCK, HOLDLOCK)
        WHERE u.UserName = p.UserName
    );

    DECLARE @ResolvedPersonnel TABLE
    (
        PersonType char(1) NOT NULL,
        UserId int NOT NULL,
        UserName nvarchar(256) NOT NULL,
        GarageId int NOT NULL,
        OperatorId int NULL,
        DriverStatusId int NULL,
        PersonnelNumber nvarchar(100) NULL,
        HolidayDay nvarchar(32) NULL
    );

    INSERT INTO @ResolvedPersonnel
        (PersonType, UserId, UserName, GarageId, OperatorId,
         DriverStatusId, PersonnelNumber, HolidayDay)
    SELECT p.PersonType, u.Id, p.UserName, p.GarageId, p.OperatorId,
           p.DriverStatusId, p.PersonnelNumber, p.HolidayDay
    FROM @Personnel p
    JOIN Users u ON u.UserName = p.UserName;

    IF EXISTS
    (
        SELECT 1
        FROM @ResolvedPersonnel p
        JOIN Drivers d WITH (UPDLOCK, HOLDLOCK)
          ON d.UserId = p.UserId OR d.PersonnelNumber = p.PersonnelNumber
        WHERE p.PersonType = 'D'
          AND
          (
              d.UserId <> p.UserId
              OR d.PersonnelNumber <> p.PersonnelNumber
              OR d.GarageId <> p.GarageId
              OR d.OperatorId <> p.OperatorId
              OR d.DriverStatusId <> p.DriverStatusId
              OR d.HolidayDay <> p.HolidayDay
          )
    )
        THROW 51012, N'Mevcut Driver kaydı beklenen kullanıcı, personel numarası, garaj, operatör, durum veya tatil günüyle uyuşmuyor.', 1;

    INSERT INTO Drivers
        (UserId, GarageId, OperatorId, DriverStatusId,
         PersonnelNumber, HolidayDay)
    SELECT p.UserId, p.GarageId, p.OperatorId, p.DriverStatusId,
           p.PersonnelNumber, p.HolidayDay
    FROM @ResolvedPersonnel p
    WHERE p.PersonType = 'D'
      AND NOT EXISTS
      (
          SELECT 1
          FROM Drivers d WITH (UPDLOCK, HOLDLOCK)
          WHERE d.UserId = p.UserId
             OR d.PersonnelNumber = p.PersonnelNumber
      );

    IF EXISTS
    (
        SELECT 1
        FROM @ResolvedPersonnel p
        JOIN Inspectors i WITH (UPDLOCK, HOLDLOCK) ON i.UserId = p.UserId
        WHERE p.PersonType = 'I'
          AND i.GarageId <> p.GarageId
    )
        THROW 51013, N'Mevcut Inspector kaydı beklenen garaja bağlı değil.', 1;

    INSERT INTO Inspectors (UserId, GarageId)
    SELECT p.UserId, p.GarageId
    FROM @ResolvedPersonnel p
    WHERE p.PersonType = 'I'
      AND NOT EXISTS
      (
          SELECT 1
          FROM Inspectors i WITH (UPDLOCK, HOLDLOCK)
          WHERE i.UserId = p.UserId
      );

    COMMIT TRANSACTION;

    SELECT N'Geliştirme personel verileri doğrulandı ve eksik kayıtlar eklendi.' AS Result;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
