
SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SearchSupplierProduct]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].SearchSupplierProduct
GO

CREATE PROCEDURE [dbo].SearchSupplierProduct
@name nvarchar(50),
@supplierId int,
@purchaseOrderId int
as


	select
	
 --   P.ProductId,

	----ProductDescription or Supplier Product Code
	--PN.ProductName 
	--+ '|' 
	--+ ProductCode
	--+  '|' 
	--+ PC.ProductColor 
	--+ '|' 
	--+ case when P.SizeId = 1 then cast(P.Width as nvarchar) + 'x' + cast(P.[Drop] as nvarchar) else isnull(cast(P.SizeId as varchar), 'n/a') end 
	--+ '|'  
	--+ 	P.ProductDescription   as
	--ProductName 


	P.ProductId,
	PN.ProductName,
	P.ProductCode,
	PC.ProductColor,
	S.Size,
	P.Width,
	P.[Drop],
	PN.ProductClassificationId,
	P.IsPerBox

	 
from Product P 
	 left join ProductName PN
	 on PN.ProductNameId = P.ProductNameId
	 left join ProductColor PC on
	 PC.ProductColorId = P.ProductColorId
	 left join Size S
	 on S.SizeId = P.SizeId

		where (


				(P.SupplierId = @supplierId
				and P.ProductId not in 
				(
					select ProductId
					from PurchaseProduct
					where PurchaseOrderId = @purchaseOrderId  and PN.ProductClassificationId = 3
				)) 
				and
				(
					(PN.ProductName like '%' + @name + '%')
				or	(P.ProductCode like '%' + @name + '%')
				or  (PC.ProductColor like '%' + @name + '%')
				
				)
			)


			Order By PN.ProductName, PC.ProductColor

	
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


--SET QUOTED_IDENTIFIER ON 
--GO
--SET ANSI_NULLS ON 
--GO

--if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[SearchSupplierProduct]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
--drop procedure [dbo].SearchSupplierProduct
--GO

--CREATE PROCEDURE [dbo].SearchSupplierProduct
--@name nvarchar(100),
--@supplierId int,
--@purchaseOrderId int
--as


--	select
	
--	P.ProductId,
--	P.ProductName + ' - ' + isnull(PC.ProductColor, ' N/A') + ' - ' + isnull(P.ProductCode, ' N/A') ProductName
	 
--from Product P left join ProductColor PC on
--	 PC.ProductColorId = P.ProductColorId

--		where (


--				(P.SupplierId = @supplierId
--				and P.ProductId not in 
--				(
--					select ProductId
--					from PurchaseProduct
--					where PurchaseOrderId = @purchaseOrderId  and ProductClassificationId = 3
--				)) 
--				and
--				(
--					(P.ProductName like '%' + @name + '%')
--				or	(P.ProductCode like '%' + @name + '%')
--				or  (PC.ProductColor like '%' + @name + '%')
				
--				)
--			)


--			Order By P.ProductName, PC.ProductColor

	
--GO
--SET QUOTED_IDENTIFIER OFF 
--GO
--SET ANSI_NULLS ON 
--GO
