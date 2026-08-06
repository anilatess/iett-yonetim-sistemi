-- Yalnızca development/test veritabanlarında isteğe bağlı olarak çalıştırılmalıdır.
-- Script idempotenttir; mevcut araçları silmez veya değiştirmez.
IF EXISTS (SELECT 1 FROM [VehicleStatuses] WHERE [Id] = 1)
BEGIN
    DECLARE @Samples TABLE
    (
        [DoorNumber] nvarchar(20), [LicensePlate] nvarchar(20),
        [Model] nvarchar(150), [ProductionYear] int, [Capacity] int
    );

    INSERT INTO @Samples VALUES
    ('A-6101', '34 IET 6101', 'OTOKAR/KENT 290LF', 2021, 98),
    ('A-6102', '34 IET 6102', 'KARSAN/AVANCITY S PLUS', 2022, 145),
    ('A-6103', '34 IET 6103', 'OTOKAR/KENT 290LF', 2020, 102),
    ('A-6104', '34 IET 6104', 'KARSAN/AVANCITY S PLUS', 2023, 150),
    ('A-6105', '34 IET 6105', 'OTOKAR/KENT 290LF', 2019, 96),
    ('A-6106', '34 IET 6106', 'KARSAN/AVANCITY S PLUS', 2021, 140),
    ('A-6107', '34 IET 6107', 'OTOKAR/KENT 290LF', 2024, 105),
    ('A-6108', '34 IET 6108', 'KARSAN/AVANCITY S PLUS', 2020, 155),
    ('A-6109', '34 IET 6109', 'OTOKAR/KENT 290LF', 2022, 100),
    ('A-6110', '34 IET 6110', 'KARSAN/AVANCITY S PLUS', 2024, 148);

    INSERT INTO [Vehicles] ([DoorNumber], [LicensePlate], [Model], [ProductionYear], [Capacity], [VehicleStatusId])
    SELECT sample.[DoorNumber], sample.[LicensePlate], sample.[Model], sample.[ProductionYear], sample.[Capacity], 1
    FROM @Samples sample
    WHERE NOT EXISTS (SELECT 1 FROM [Vehicles] vehicle WHERE vehicle.[LicensePlate] = sample.[LicensePlate])
      AND NOT EXISTS (SELECT 1 FROM [Vehicles] vehicle WHERE vehicle.[DoorNumber] = sample.[DoorNumber]);
END;
