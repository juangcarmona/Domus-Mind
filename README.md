# DomusMind

**DomusMind** is an open project to reduce the invisible mental load of managing a household.

Families coordinate a huge number of things every day: schedules, responsibilities, tasks, routines, children, pets, events, logistics, and planning. Most of this coordination is informal, fragmented, and cognitively expensive.

DomusMind aims to make the **operating system of a household explicit**.

---

## The Idea

A home is a **system of responsibilities, time, and coordination**.

DomusMind models that system with four core domains:

- **Family** - household structure and members  
- **Responsibilities** - ownership of household domains  
- **Calendar** - events and shared time  
- **Tasks** - execution of work and routines  

The goal is simple:

> make household coordination visible, shared, and manageable.

---

## Project Status

Early architecture phase.

The repository currently contains:

- product vision and principles
- domain model
- bounded contexts
- architectural decisions
- vertical slice specifications
- system roadmap

Implementation is beginning.

---

## Architecture

DomusMind is designed as a **modular monolith with vertical slices**.

Key ideas:

- bounded contexts
- explicit aggregates
- command/query separation
- REST API
- EF Core with projection-based queries
- append-only domain event log

The system is intentionally simple and evolvable.

---

## Goals

DomusMind is trying to achieve three things:

1. **Reduce mental load** in household coordination
2. **Make responsibilities explicit**
3. **Create a shared operational view of family life**

---

## Contributing

This project is in its early stage and **contributors are welcome**.

Ways to help:

- architecture feedback
- domain modeling discussions
- implementation
- UX ideas
- documentation improvements

If you are interested, open an issue or start a discussion.

---

## License

This project is licensed under the **MIT License**.

You are free to:

- use it
- modify it
- distribute it
- build on top of it
- use it commercially

The only requirement is **preserving attribution**.

DomusMind was originally created by [Juan G. Carmona](https://jgcarmona.com) for his own family, and is released as a gift to the world so others can build on it, improve it, and use it to make household life easier.