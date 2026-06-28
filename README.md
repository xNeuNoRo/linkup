**LinkUp Pro** es una plataforma web integral que combina una red social completa con un motor de minijuegos interactivos (Battleship). Desarrollada como proyecto universitario para el Instituto Tecnológico de las Américas (ITLA), la aplicación está construida sobre **.NET 9** utilizando **Clean Architecture** y **Domain-Driven Design (DDD)**.

## 📑 Tabla de Contenidos

1. [Stack Tecnológico](#stack-tecnol%C3%B3gico)

2. [Estructura del Proyecto](#estructura-del-proyecto)

3. [Requisitos Previos](#requisitos-previos)

4. [Configuración del Entorno Local](#configuraci%C3%B3n-del-entorno-local)

5. [Migraciones y Base de Datos](#migraciones-y-base-de-datos)

6. [Credenciales por Defecto (Seed Data)](#credenciales-por-defecto-seed-data)

7. [Ejecución de Pruebas (TDD)](#ejecuci%C3%B3n-de-pruebas-tdd)

## 💻 Stack Tecnológico

* **Framework Core:** .NET 9.0 (C# 13)

* **Arquitectura:** Clean Architecture / Onion Architecture

* **Presentación:** ASP.NET Core MVC (Razor Views), HTML5, Bootstrap 5, CSS3, Vanilla JS

* **Persistencia de Datos:** SQL Server, Entity Framework Core 9.0 (Code-First)

* **Autenticación y Seguridad:** ASP.NET Core Identity (Autenticación basada en Cookies)

* **Mapeo de Objetos:** Mapster

* **Validaciones:** FluentValidation

* **Envío de Correos:** MailKit / MimeKit (Implementación asíncrona)

* **Testing:** xUnit, Moq, FluentAssertions

## 🏗 Estructura del Proyecto

El proyecto está dividido estrictamente en capas para garantizar un bajo acoplamiento y alta cohesión. **Por favor, lee el archivo&#x20;**`ARCHITECTURE_GUIDELINES.md`**&#x20;antes de escribir código.**

* `LinkUpPro.Core.Domain`: Entidades, Enums, Excepciones y contratos (interfaces). Sin dependencias externas.

* `LinkUpPro.Core.Application`: Casos de uso (Servicios), DTOs, ViewModels y Validaciones.

* `LinkUpPro.Infrastructure.Persistence`: Implementación de EF Core, AppDbContext, Repositorios genéricos y Migraciones.

* `LinkUpPro.Infrastructure.Identity`: Gestión de usuarios, roles y autenticación con ASP.NET Core Identity.

* `LinkUpPro.Infrastructure.Shared`: Servicios transversales (Envío de correos, gestión de archivos locales).

* `LinkUpPro.WebApp`: Capa de presentación (Controladores, Vistas Razor, Middlewares).

## 🛠 Requisitos Previos

Antes de clonar el repositorio, asegúrate de tener instalado en tu máquina:

1. [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) o superior.

2. **Microsoft SQL Server** (Developer o Express edition) local o ejecutándose en un contenedor Docker.

3. **IDE Recomendado:** Visual Studio 2022 (v17.12+), JetBrains Rider, o VS Code (con la extensión de C# Dev Kit).

4. Herramientas de CLI de EF Core instaladas globalmente. Si no las tienes, ejecuta en tu terminal:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

## 🚀 Configuración del Entorno Local

**1. Clonar el repositorio:**

```bash
git clone [https://github.com/tu-usuario/LinkUpPro.git](https://github.com/tu-usuario/LinkUpPro.git)
cd LinkUpPro
```

**2. Configurar el archivo&#x20;**`appsettings.Development.json`**:**\
Ve al proyecto `LinkUpPro.WebApp` y crea/modifica el archivo `appsettings.Development.json` para establecer tu cadena de conexión y credenciales SMTP (para el envío de correos de activación).

**EL SIGUIENTE TEXTO NO REFLEJA EL ARCHIVO appsettings.Development.json FINAL, SE IRA MODIFICANDO A MEDIDA QUE AVANCEMOS CON EL DESARROLLO.**

```json
{
  "ConnectionStrings": {
    "LinkUpDb": "Server=localhost\\SQLEXPRESS;Database=LinkUpProDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "MailSettings": {
    "EmailFrom": "noreply@linkuppro.com",
    "SmtpHost": "smtp.tuserver.com",
    "SmtpPort": 587,
    "SmtpUser": "tu_correo@gmail.com",
    "SmtpPass": "tu_password_de_aplicacion",
    "DisplayName": "LinkUp Pro System"
  }
}
```

## 🗄 Migraciones y Base de Datos

⚠ **IMPORTANTE:** Debido a la Clean Architecture, el `DbContext` **no** está en el proyecto Web. Las migraciones deben apuntar a los proyectos de infraestructura.

Abre la terminal en la raíz de la solución (`.sln`) y ejecuta el siguiente comando para aplicar la base de datos de Aplicación (Persistencia):

```bash
dotnet ef database update --project src/LinkUpPro.Infrastructure.Persistence --startup-project src/LinkUpPro.WebApp
```

Si el proyecto utiliza un contexto separado para Identity (`IdentityContext`), ejecuta también:

```bash
dotnet ef database update --context IdentityContext --project src/LinkUpPro.Infrastructure.Identity --startup-project src/LinkUpPro.WebApp
```

## 🔑 Credenciales por Defecto (Seed Data)

Al ejecutar la aplicación por primera vez, el `DbSeeder` creará automáticamente los roles básicos y un par de usuarios para que no tengas que registrarte manualmente durante las pruebas.

**Usuario Administrador (Testing general):**

* **Email:** admin@linkuppro.com

* **Password:** 123Pa$$word!

**Usuario Estándar (Jugador 1):**

* **Email:** player1@linkuppro.com

* **Password:** 123Pa$$word!

_(Nota: Estas credenciales solo se siembran en entornos de desarrollo)._

## 🧪 Ejecución de Pruebas (TDD)

El proyecto sigue un enfoque **Interface-First**. El Tech Lead creará las interfaces y pruebas unitarias en `LinkUpPro.Tests` que fallarán inicialmente. Tu trabajo como desarrollador es implementar la lógica en `Application` para que las pruebas pasen en verde.

Para ejecutar todas las pruebas de la solución, abre tu terminal y ejecuta:

```bash
dotnet test
```

Para ver el output detallado de una prueba específica que esté fallando:

```bash
dotnet test --logger "console;verbosity=detailed"
```

## 🤝 Flujo de Trabajo y Contribuciones

* Revisa tus tareas asignadas en **Plane**.

* Crea una rama a partir de `development` con el formato `feature/nombre-de-tarea` o `fix/nombre-del-bug`.

* Escribe el código respetando las normativas de `ARCHITECTURE_GUIDELINES.md`.

* Asegúrate de que todas las pruebas pasen (`dotnet test`).

* Abre un **Pull Request (PR)** hacia `development`. No hagas merge directamente; requiere la aprobación del Tech Lead (Code Review).
