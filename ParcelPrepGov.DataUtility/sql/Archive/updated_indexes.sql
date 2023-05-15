GO
CREATE NONCLUSTERED INDEX [IX_PS_SC_LPD_StopTheClockEventDate>]
ON [dbo].[PackageDatasets] ([PackageStatus],[ShippingCarrier],[LocalProcessedDate])
INCLUDE ([StopTheClockEventDate])
GO

GO
CREATE NONCLUSTERED INDEX [<Ix_PS_SC_LPD_StopTheClockEventDate>]
ON [dbo].[PackageDatasets] ([PackageStatus],[ShippingCarrier],[LocalProcessedDate])
INCLUDE ([StopTheClockEventDate])
GO

GO
CREATE NONCLUSTERED INDEX [<IX_ClientName_PS_CS_LPD_StopTheClockEventDate>]
ON [dbo].[PackageDatasets] ([ClientName],[PackageStatus],[ShippingCarrier],[LocalProcessedDate])
INCLUDE ([StopTheClockEventDate])
GO