class PromptGenException(Exception):
    """Base exception for PromptGen API"""
    pass


class ValidationError(PromptGenException):
    """Validation error"""
    pass


class AuthenticationError(PromptGenException):
    """Authentication error"""
    pass


class AuthorizationError(PromptGenException):
    """Authorization error"""
    pass


class RateLimitError(PromptGenException):
    """Rate limit exceeded error"""
    pass


class ModelError(PromptGenException):
    """AI model error"""
    pass


class StorageError(PromptGenException):
    """Storage error"""
    pass


class CacheError(PromptGenException):
    """Cache error"""
    pass


class NotFoundError(PromptGenException):
    """Resource not found error"""
    pass


class ConflictError(PromptGenException):
    """Resource conflict error"""
    pass


class ServiceUnavailableError(PromptGenException):
    """Service unavailable error"""
    pass
