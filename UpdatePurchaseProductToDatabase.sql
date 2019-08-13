

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UpdatePurchaseProductToDatabase]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[UpdatePurchaseProductToDatabase]
GO

CREATE PROCEDURE [dbo].[UpdatePurchaseProductToDatabase]
@purchaseOrderId int,
@userId int
as

SET XACT_ABORT ON
begin transaction updateInventory

	declare @list table
	(

	rowId int identity(1,1) not null primary key,
	PurchaseOrderId int,
	ProductId int,
	ProductClassificationId int,
	Quantity int,
	QuantityOnBox int,
	QuantityPerBox int,
	IsPerBox bit,
	Width int,
	[Drop] int
	)

	-- insert data with received status

	insert into @list
	(
	PurchaseOrderId,
	ProductId,
	ProductClassificationId,
	Quantity,
	QuantityOnBox,
	QuantityPerBox,
	IsPerBox,
	Width,
	[Drop]
	)

	select
	PP.PurchaseOrderId,
	P.ProductId,
	PC.ProductClassificationId,
	PP.Quantity,
	PP.QuantityOnBox,
	P.QuantityPerBox,
	P.IsPerBox,
	PP.Width,
	PP.[Drop]

	from PurchaseProduct PP 
	inner join Product P on
	P.ProductId = PP.ProductId
	inner join ProductClassification PC on
	PC.ProductClassificationId = P.ProductClassificationId
	where
	PP.PurchaseOrderId = @purchaseOrderId and PP.PurchaseProductStatusId = 3 -- check finish product, not included

	

	declare @max int
	declare @pk int
	declare @id int 

	declare @productIdList int
	declare @quantityList int
	declare @quantityOnBoxList int
	declare @quantityPerBoxList int
	declare @isPerBoxList bit
	declare @widthList int
	declare @dropList int

	declare @productClassificationId int

	declare @productId int
	--declare @quantity int
	--declare @quantityOnBox int
	declare @quantityPerBox int
	declare @productInventoryId int


	
	set @max = (select max(rowid) from @list)
	set @pk = 1

	while @pk <= @max

		begin

			declare @newId int = null
			declare @quantity int = null
			declare @quantityOnBox int = null

			select
				@productIdList = ProductId,
				@widthList = Width,
				@droplist = [Drop],
				@quantityList = Quantity,
				@productClassificationId = ProductClassificationId
			from @list where rowId = @pk

			-- fabric products
			if(@productClassificationId = 2)
			begin
				insert into Fabric
				(
					PurchaseOrderId,
					ProductId,
					Width,
					[Drop],
					Measurement,
					Quantity
						)
				values
				(
					@purchaseOrderId,
					@productIdList,
					@widthList,
					@dropList,
					convert(nvarchar(10), @widthList) + ' x ' + cast(@dropList as nvarchar(10)),
					@quantityList
				)

				if @@ERROR <> 0
					begin
						raiserror('failed on update', 16, 1)
						rollback transaction updateInventory
						return -1
					end 
			end
			-- Other Product
			else if(@productClassificationId = 3)
			begin
				
				select 
					@quantityList = Quantity,
					@quantityOnBoxList = QuantityOnBox,
					@quantityPerBoxList = QuantityPerBox,
					@isPerBoxList = IsPerBox
				from @list where rowId = @pk

			
				select 
					@quantity = Quantity,
					@quantityOnBox = QuantityOnbox,
					@productInventoryId = ProductInventoryId
				from ProductInventory where ProductId = @productIdList

				declare @quotient int = 0
				declare @remainder int
				

				if(@isPerBoxList = 1 and @quantityList is not null)
				begin
					set @quotient = (@quantityList + isnull(@quantity, 0))  / @quantityPerBoxList
					set @remainder = (@quantityList + isnull(@quantity, 0))  % @quantityPerBoxList
				end


			    -- New product - insert to table
				if not exists(select * from ProductInventory where ProductId = @productIdList)
				begin
					insert into ProductInventory
					(
						ProductId,
						Quantity,
						QuantityOnBox
					)
					values
					(
						@productIdList,
						case when @quotient = 0 then isnull(@quantityList, 0) else @remainder end, 
						case when @quotient = 0 then isnull(@quantityOnBoxList, 0) else @quotient + isnull(@quantityOnBoxList, 0) end
					)

					if @@ERROR <> 0
					begin
						raiserror('failed on update', 16, 1)
						rollback transaction updateInventory
						return -1
					end 
						--New ProductInventoryId
				--declare @newId int = null
				set @newId = SCOPE_IDENTITY()

				end

				
				

				declare @quantityLeft int
				declare @quantityOnBoxLeft int

			
				
				if(@quotient <> 0) -- if its working - test case when
				begin
					set @quantityOnBoxLeft = isnull(@quantityOnBox, 0) + @quotient + isnull(@quantityOnBoxList, 0)
					set @quantityLeft = @remainder
				end
				else
				begin
					set @quantityOnBoxLeft = isnull(@quantityOnBox, 0) + isnull(@quantityOnBoxList, 0)
					set @quantityLeft = isnull(@quantity, 0) + isnull(@quantityList, 0)
				end


				insert into ProductInventoryLog
				(
					ProductInventoryId,
					PurchaseOrderId,
					Quantity,
					QuantityOnBox,
					QuantityInventory,
					QuantityOnBoxInventory,
					QuantityLeft,
					QuantityOnBoxLeft,
					UserId,
					DateCreated
				)
				values
				(
					case when @newId > 0 then @newId else @productInventoryId end,
					@purchaseOrderId,
					isnull(@quantityList,0),
					isnull(@quantityOnBoxList, 0),
					isnull(@quantity, 0),
					isnull(@quantityOnBox, 0),
					@quantityLeft,
					@quantityOnBoxLeft,
					@userId,
					GETDATE()
				)

				if @@ERROR <> 0
					begin
						raiserror('failed on update', 16, 1)
						rollback transaction updateInventory
						return -1
					end 

				if(@newId is null)
				begin
					Update ProductInventory
					set
						Quantity = @quantityLeft,
						QuantityOnbox = @quantityOnBoxLeft
					where  ProductId = @productIdList

					if @@ERROR <> 0
						begin
						raiserror('failed on update', 16, 1)
						rollback transaction updateInventory
						return -1
					end 
				end
			end
			set @pk = @pk + 1
		end

update PurchaseOrder
	set IsUpdatedToDB = 1
	where PurchaseOrderId = @purchaseOrderId
	if @@ERROR <> 0
	begin
		raiserror('failed on update', 16, 1)
		rollback transaction updateInventory
		return -1
	end 
commit transaction updateInventory
return 1


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
