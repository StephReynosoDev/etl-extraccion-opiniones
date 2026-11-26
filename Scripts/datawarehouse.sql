IF DB_ID('DataWarehouse') IS NULL
    CREATE DATABASE DataWarehouse;
GO

USE DataWarehouse;
GO

IF OBJECT_ID('dbo.DimCliente', 'U') IS NULL
    CREATE TABLE dbo.DimCliente (
        IdClienteDW INT IDENTITY(1,1) PRIMARY KEY,
        IdClienteOrigen INT NOT NULL UNIQUE,
        Nombre VARCHAR(100),
        Correo VARCHAR(100),
        FechaCarga DATETIME NOT NULL DEFAULT GETDATE()
    );
GO

IF OBJECT_ID('dbo.DimProducto', 'U') IS NULL
    CREATE TABLE dbo.DimProducto (
        IdProductoDW INT IDENTITY(1,1) PRIMARY KEY,
        IdProductoOrigen INT NOT NULL UNIQUE,
        Nombre VARCHAR(100),
        Categoria VARCHAR(50),
        FechaCarga DATETIME NOT NULL DEFAULT GETDATE()
    );
GO

IF OBJECT_ID('dbo.DimFuente', 'U') IS NULL
    CREATE TABLE dbo.DimFuente (
        IdFuenteDW INT IDENTITY(1,1) PRIMARY KEY,
        IdFuenteOrigen INT NOT NULL UNIQUE,
        Nombre VARCHAR(50),
        FechaCarga DATETIME NOT NULL DEFAULT GETDATE()
    );
GO

IF OBJECT_ID('dbo.DimTiempo', 'U') IS NULL
    CREATE TABLE dbo.DimTiempo (
        IdTiempo INT PRIMARY KEY,
        Fecha DATE NOT NULL UNIQUE,
        Anio INT NOT NULL,
        Trimestre INT NOT NULL,
        Mes INT NOT NULL,
        NombreMes VARCHAR(15) NOT NULL,
        Dia INT NOT NULL,
        NombreDia VARCHAR(15) NOT NULL
    );
GO

IF OBJECT_ID('dbo.DimSentimiento', 'U') IS NULL
    CREATE TABLE dbo.DimSentimiento (
        IdSentimiento INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(20) NOT NULL UNIQUE,
        Descripcion VARCHAR(100)
    );
GO

IF OBJECT_ID('dbo.FactOpinion', 'U') IS NULL
    CREATE TABLE dbo.FactOpinion (
        IdFactOpinion INT IDENTITY(1,1) PRIMARY KEY,
        IdOpinionOrigen INT NOT NULL UNIQUE,
        IdTiempo INT NOT NULL FOREIGN KEY REFERENCES dbo.DimTiempo(IdTiempo),
        IdClienteDW INT NOT NULL FOREIGN KEY REFERENCES dbo.DimCliente(IdClienteDW),
        IdProductoDW INT NOT NULL FOREIGN KEY REFERENCES dbo.DimProducto(IdProductoDW),
        IdFuenteDW INT NOT NULL FOREIGN KEY REFERENCES dbo.DimFuente(IdFuenteDW),
        IdSentimiento INT NULL FOREIGN KEY REFERENCES dbo.DimSentimiento(IdSentimiento),
        Calificacion INT
    );
GO

CREATE OR ALTER PROCEDURE sp_CargarDimCliente
    @id INT,
    @nombre VARCHAR(100),
    @correo VARCHAR(100)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DimCliente WHERE IdClienteOrigen = @id)
    BEGIN
        INSERT INTO DimCliente (IdClienteOrigen, Nombre, Correo) VALUES (@id, @nombre, @correo);
        SELECT 'INSERTADO';
    END
    ELSE IF EXISTS (SELECT 1 FROM DimCliente WHERE IdClienteOrigen = @id
                    AND (ISNULL(Nombre, '') <> ISNULL(@nombre, '') OR ISNULL(Correo, '') <> ISNULL(@correo, '')))
    BEGIN
        UPDATE DimCliente SET Nombre = @nombre, Correo = @correo, FechaCarga = GETDATE() WHERE IdClienteOrigen = @id;
        SELECT 'ACTUALIZADO';
    END
    ELSE
        SELECT 'SIN CAMBIOS';
END
GO

CREATE OR ALTER PROCEDURE sp_CargarDimProducto
    @id INT,
    @nombre VARCHAR(100),
    @categoria VARCHAR(50)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DimProducto WHERE IdProductoOrigen = @id)
    BEGIN
        INSERT INTO DimProducto (IdProductoOrigen, Nombre, Categoria) VALUES (@id, @nombre, @categoria);
        SELECT 'INSERTADO';
    END
    ELSE IF EXISTS (SELECT 1 FROM DimProducto WHERE IdProductoOrigen = @id
                    AND (ISNULL(Nombre, '') <> ISNULL(@nombre, '') OR ISNULL(Categoria, '') <> ISNULL(@categoria, '')))
    BEGIN
        UPDATE DimProducto SET Nombre = @nombre, Categoria = @categoria, FechaCarga = GETDATE() WHERE IdProductoOrigen = @id;
        SELECT 'ACTUALIZADO';
    END
    ELSE
        SELECT 'SIN CAMBIOS';
END
GO

CREATE OR ALTER PROCEDURE sp_CargarDimFuente
    @id INT,
    @nombre VARCHAR(50)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DimFuente WHERE IdFuenteOrigen = @id)
    BEGIN
        INSERT INTO DimFuente (IdFuenteOrigen, Nombre) VALUES (@id, @nombre);
        SELECT 'INSERTADO';
    END
    ELSE IF EXISTS (SELECT 1 FROM DimFuente WHERE IdFuenteOrigen = @id AND ISNULL(Nombre, '') <> ISNULL(@nombre, ''))
    BEGIN
        UPDATE DimFuente SET Nombre = @nombre, FechaCarga = GETDATE() WHERE IdFuenteOrigen = @id;
        SELECT 'ACTUALIZADO';
    END
    ELSE
        SELECT 'SIN CAMBIOS';
END
GO

CREATE OR ALTER PROCEDURE sp_CargarDimTiempo
    @fecha DATE
AS
BEGIN
    IF EXISTS (SELECT 1 FROM DimTiempo WHERE Fecha = @fecha)
        SELECT 'SIN CAMBIOS';
    ELSE
    BEGIN
        INSERT INTO DimTiempo (IdTiempo, Fecha, Anio, Trimestre, Mes, NombreMes, Dia, NombreDia)
        VALUES (
            YEAR(@fecha) * 10000 + MONTH(@fecha) * 100 + DAY(@fecha),
            @fecha,
            YEAR(@fecha),
            DATEPART(QUARTER, @fecha),
            MONTH(@fecha),
            CHOOSE(MONTH(@fecha), 'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'),
            DAY(@fecha),
            CHOOSE((DATEDIFF(DAY, '19000101', @fecha) % 7) + 1, 'Lunes', 'Martes', 'Miercoles', 'Jueves', 'Viernes', 'Sabado', 'Domingo')
        );
        SELECT 'INSERTADO';
    END
END
GO

CREATE OR ALTER PROCEDURE sp_CargarDimSentimiento
    @nombre VARCHAR(20),
    @descripcion VARCHAR(100)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DimSentimiento WHERE Nombre = @nombre)
    BEGIN
        INSERT INTO DimSentimiento (Nombre, Descripcion) VALUES (@nombre, @descripcion);
        SELECT 'INSERTADO';
    END
    ELSE IF EXISTS (SELECT 1 FROM DimSentimiento WHERE Nombre = @nombre AND ISNULL(Descripcion, '') <> ISNULL(@descripcion, ''))
    BEGIN
        UPDATE DimSentimiento SET Descripcion = @descripcion WHERE Nombre = @nombre;
        SELECT 'ACTUALIZADO';
    END
    ELSE
        SELECT 'SIN CAMBIOS';
END
GO
