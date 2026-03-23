# DomusMind — API

The API exposes explicit household capabilities through ASP.NET Core controllers.

## Core Resources

- families
- events
- tasks
- routines
- member activity
- household areas

## Areas

Areas are exposed as lightweight read models.
They contain minimal presentation data such as `id`, `name`, and optional `color`.
They do not expose ownership or assignment structures.
