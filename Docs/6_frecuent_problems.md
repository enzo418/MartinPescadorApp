# Frecuent problems that might occur at run time
## Doesn't hit validator
After adding a `AbstractValidator<T>` for a `Command<Action> : IRequest<...>` it might not be hit. This is because the command needs to inherit from `IRequest<ErrorOr<...>>`.
I added a function to verify this in the setup process of the application. It will throw an exception. *Thanks tests*.