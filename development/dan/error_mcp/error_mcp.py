from typing import Callable, Dict, Any
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("ErrorMCP")

class ErrorMCP:
    """
    A general-purpose error manager for lab automation systems.
    Handles error routing, logging, and device-specific recovery strategies.
    """

    def __init__(self):
        self.handlers: Dict[str, Callable[[str], Dict[str, Any]]] = {}

    def register_handler(self, device_name: str, handler_fn: Callable[[str], Dict[str, Any]]) -> None:
        self.handlers[device_name.lower()] = handler_fn
        logger.info(f"Handler registered for device: {device_name}")

    def handle_error(self, device_name: str, error_msg: str) -> Dict[str, Any]:
        device_name = device_name.lower()
        if device_name not in self.handlers:
            logger.warning(f"No handler registered for {device_name}")
            return {"status": "unhandled", "message": error_msg}

        try:
            logger.info(f"Handling error for {device_name}: {error_msg}")
            return self.handlers[device_name](error_msg)
        except Exception as e:
            logger.exception(f"Handler for {device_name} failed")
            return {"status": "handler_exception", "message": str(e)}
