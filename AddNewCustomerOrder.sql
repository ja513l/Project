
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[AddNewCustomerOrder]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[AddNewCustomerOrder]
GO

CREATE PROCEDURE [dbo].[AddNewCustomerOrder]
 
 @lastName nvarchar(25),
 @firstName nvarchar(25),
 @address nvarchar(50),
 @contactNumber nvarchar(20),
 @companyName nvarchar(25),
 @customerTypeId int,
 @email nvarchar(25),
 @userId int
 
as
SET XACT_ABORT ON
begin transaction addNewCustomerOrder
	insert into Customer
	(
		LastName,
		FirstName,
		[Address],
		ContactNumber,
		CompanyName,
		CustomerTypeId,
		Email
	)

	values
	(
		 @lastName,
		 @firstName,
		 @address,
		 @contactNumber,
		 @companyName,
		 @customerTypeId,
		 @email
	)

		if @@ERROR <> 0
		begin
			raiserror('failed on add ', 16, 1)
			rollback transaction addNewCustomerOrder
			return -1
		end


	declare @customerId int 
	set @customerId = SCOPE_IDENTITY()

	insert into CustomerOrder
	(
		CustomerId,
		StatusId,
		OrderTypeId,
		InstallationFee,
		DateCreated,
		CreatedBy,
		OrderDate
		--TotalAmountPaid
	)
	values
	(
		@customerId,
		1,
		@customerTypeId,
		0.00,
		getdate(),
		@userId,
		GETDATE()
		--0.00
	)

	if @@ERROR <> 0
		begin
			raiserror('failed on add ', 16, 1)
			rollback transaction addNewCustomerOrder
			return -1
		end

	declare @orderNumber int
	set @orderNumber = SCOPE_IDENTITY();

	insert into UsedCustomerProduct
	(
		CustomerOrderId,
		IsUpdateToDB
	)
	values
	(
		@orderNumber,
		0
	)

	if @@ERROR <> 0
		begin
			raiserror('failed on add ', 16, 1)
			rollback transaction addNewCustomerOrder
			return -1
		end

commit transaction addNewCustomerOrder

return @orderNumber


