namespace MyMoviesApp.Domain.Exceptions;

public class DuplicateEmailException(string email)
    : InvalidOperationException($"A user with email '{email}' already exists.");

