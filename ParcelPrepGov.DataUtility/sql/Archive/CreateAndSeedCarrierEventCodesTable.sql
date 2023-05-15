CREATE TABLE [CarrierEventCodes] (
    [Id] int NOT NULL IDENTITY,
    [CreateDate] datetime2 NOT NULL,
    [ShippingCarrier] varchar(50) NULL,
    [Code] varchar(10) NULL,
    [Description] varchar(50) NULL,
    [IsStopTheClock] int NOT NULL,
    [IsUndeliverable] int NOT NULL,
    CONSTRAINT [PK_CarrierEventCodes] PRIMARY KEY ([Id])
);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211122165454_AddCarrierEventCodeTabl', N'3.1.6');

GO
 
if(Exists(select * from information_schema.tables 
 where table_schema = 'dbo' and table_name = 'CarrierEventCodes'))
BEGIN
INSERT INTO [dbo].[CarrierEventCodes]
           ([CreateDate]
           ,[Description]
           ,[Code]
           ,[ShippingCarrier]
           ,[IsStopTheClock]
           ,[IsUndeliverable])
     VALUES
           ('11/22/2021', 'AT CANADA POST FACILITY', 'AC', 'FEDEX', 0, 0),
		   ('11/22/2021', 'AT LOCAL FACILITY', 'AF', 'FEDEX', 0, 0),
		   ('11/22/2021', 'AT PICKUP', 'AP', 'FEDEX', 0, 0),
		   ('11/22/2021', 'ARIVED AT FEDEX LOCATION', 'AR', 'FEDEX', 0, 0),
		   ('11/22/2021', 'TENDERED TO USPS FOR DELIVERY', 'AX', 'FEDEX', 0, 0),
		   ('11/22/2021', 'SHIPMENT CANCELLED BY SENDER', 'CA', 'FEDEX', 0, 0),
		   ('11/22/2021', 'INTERNATIONAL SHIPMENT RELEASE', 'CC', 'FEDEX', 0, 0),
		   ('11/22/2021', 'CLEARANCE DELAY', 'CD', 'FEDEX', 0, 0),
		   ('11/22/2021', 'CLEARANCE IN PROGRESS', 'CP', 'FEDEX', 0, 0),
		   ('11/22/2021', 'DELIVERY EXCEPTION', 'DE', 'FEDEX', 0, 0),
		   ('11/22/2021', 'DELIVERED', 'DL', 'FEDEX', 1, 0),
		   ('11/22/2021', 'DEPARTED FEDEX LOCATION', 'DP', 'FEDEX', 0, 0),
		   ('11/22/2021', 'US EXPORT APPROVED', 'EA', 'FEDEX', 0, 0),
		   ('11/22/2021', 'AT FEDEX DESTINATION', 'FD', 'FEDEX', 0, 0),
		   ('11/22/2021', 'HELD FOR PICKUP', 'HL', 'FEDEX', 0, 0),
		   ('11/22/2021', 'IN FEDEX POSSESSION', 'IP', 'FEDEX', 0, 0),
		   ('11/22/2021', 'IN TRANSIT', 'IT', 'FEDEX', 0, 0),
		   ('11/22/2021', 'IN TRANSIT', 'IX', 'FEDEX', 0, 0),
		   ('11/22/2021', 'IN TRANSIT', 'LO', 'FEDEX', 0, 0),
		   ('11/22/2021', 'PACKAGE DATA TRANSMITTED TO FEDEX', 'OC', 'FEDEX', 0, 0),
		   ('11/22/2021', 'ON FEDEX VEHICLE FOR DELIVERY', 'OD', 'FEDEX', 0, 0),
		   ('11/22/2021', 'AT FEDEX ORIGIN FACILITY', 'OF', 'FEDEX', 0, 0),
		   ('11/22/2021', 'PACKAGE DATA TRANSMITTED BY USPS', 'OX', 'FEDEX', 0, 0),
		   ('11/22/2021', 'PICKED UP', 'PU', 'FEDEX', 0, 0),
		   ('11/22/2021', 'PICKED UP', 'PX', 'FEDEX', 0, 0),
		   ('11/22/2021', 'SHIPMENT EXCEPTION', 'SE', 'FEDEX', 0, 0),
		   ('11/22/2021', 'AT DESTINATION SORT FACILITY', 'SF', 'FEDEX', 0, 0),
		   ('11/22/2021', 'IN TRANSIT', 'TR', 'FEDEX', 0, 0)
END
GO
 