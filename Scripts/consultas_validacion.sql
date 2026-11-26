USE DataWarehouse;
GO

SELECT 'DimCliente' AS Tabla, COUNT(*) AS TotalRegistros FROM DimCliente
UNION ALL
SELECT 'DimProducto', COUNT(*) FROM DimProducto
UNION ALL
SELECT 'DimFuente', COUNT(*) FROM DimFuente
UNION ALL
SELECT 'DimTiempo', COUNT(*) FROM DimTiempo
UNION ALL
SELECT 'DimSentimiento', COUNT(*) FROM DimSentimiento;
GO

SELECT 'Cliente' AS Dimension,
       (SELECT COUNT(*) FROM OpinionesClientes.dbo.Cliente) AS Origen,
       (SELECT COUNT(*) FROM DimCliente) AS DataWarehouse
UNION ALL
SELECT 'Producto',
       (SELECT COUNT(*) FROM OpinionesClientes.dbo.Producto),
       (SELECT COUNT(*) FROM DimProducto)
UNION ALL
SELECT 'Fuente',
       (SELECT COUNT(*) FROM OpinionesClientes.dbo.FuentesDatos),
       (SELECT COUNT(*) FROM DimFuente);
GO

SELECT TOP 10 * FROM DimCliente;
SELECT TOP 10 * FROM DimProducto;
SELECT * FROM DimFuente;
SELECT * FROM DimSentimiento;
GO

SELECT Categoria, COUNT(*) AS TotalProductos
FROM DimProducto
GROUP BY Categoria
ORDER BY TotalProductos DESC;
GO

SELECT Anio, COUNT(*) AS TotalDias, MIN(Fecha) AS PrimeraFecha, MAX(Fecha) AS UltimaFecha
FROM DimTiempo
GROUP BY Anio
ORDER BY Anio;
GO

SELECT TOP 10 * FROM DimTiempo ORDER BY Fecha;
GO
