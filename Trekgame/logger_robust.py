from collections import deque
from datetime import datetime
import io
import logging
import sys
import threading

logs = None
stdout_interceptor = None
stderr_interceptor = None


class SafeStreamWrapper:
    """A wrapper that catches all IO errors and never crashes."""

    def __init__(self, stream):
        self._stream = stream
        self._lock = threading.Lock()
        self._flush_callbacks = []
        self._logs_since_flush = []
        # Cache stream properties
        try:
            self.encoding = getattr(stream, 'encoding', 'utf-8') or 'utf-8'
        except:
            self.encoding = 'utf-8'
        try:
            self.errors = getattr(stream, 'errors', 'replace') or 'replace'
        except:
            self.errors = 'replace'

    def write(self, data):
        global logs
        entry = {"t": datetime.now().isoformat(), "m": str(data)}
        with self._lock:
            self._logs_since_flush.append(entry)
            try:
                if logs is not None:
                    if isinstance(data, str) and data.startswith("\r") and len(logs) > 0:
                        try:
                            if not logs[-1]["m"].endswith("\n"):
                                logs.pop()
                        except:
                            pass
                    logs.append(entry)
            except:
                pass
        try:
            result = self._stream.write(data)
            return result
        except:
            return len(str(data)) if data else 0

    def flush(self):
        try:
            self._stream.flush()
        except:
            pass
        callbacks = list(self._flush_callbacks)
        logs_copy = list(self._logs_since_flush)
        self._logs_since_flush = []
        for cb in callbacks:
            try:
                cb(logs_copy)
            except:
                pass

    def on_flush(self, callback):
        self._flush_callbacks.append(callback)

    def fileno(self):
        try:
            return self._stream.fileno()
        except:
            raise io.UnsupportedOperation("fileno")

    def isatty(self):
        try:
            return self._stream.isatty()
        except:
            return False

    def readable(self):
        return False

    def writable(self):
        return True

    def seekable(self):
        return False

    def close(self):
        pass  # Never close

    def __getattr__(self, name):
        # Forward unknown attributes to the wrapped stream
        try:
            return getattr(self._stream, name)
        except:
            raise AttributeError(name)


def get_logs():
    return logs


def on_flush(callback):
    if stdout_interceptor is not None:
        stdout_interceptor.on_flush(callback)
    if stderr_interceptor is not None:
        stderr_interceptor.on_flush(callback)


def setup_logger(log_level: str = 'INFO', capacity: int = 300, use_stdout: bool = False):
    global logs
    if logs:
        return

    logs = deque(maxlen=capacity)

    global stdout_interceptor
    global stderr_interceptor

    # Use our safe wrapper instead of TextIOWrapper
    stdout_interceptor = SafeStreamWrapper(sys.stdout)
    stderr_interceptor = SafeStreamWrapper(sys.stderr)
    sys.stdout = stdout_interceptor
    sys.stderr = stderr_interceptor

    logger = logging.getLogger()
    logger.setLevel(log_level)

    stream_handler = logging.StreamHandler()
    stream_handler.setFormatter(logging.Formatter("%(message)s"))

    if use_stdout:
        stream_handler.addFilter(lambda record: not record.levelno < logging.ERROR)
        stdout_handler = logging.StreamHandler(sys.stdout)
        stdout_handler.setFormatter(logging.Formatter("%(message)s"))
        stdout_handler.addFilter(lambda record: record.levelno < logging.ERROR)
        logger.addHandler(stdout_handler)

    logger.addHandler(stream_handler)


STARTUP_WARNINGS = []


def log_startup_warning(msg):
    logging.warning(msg)
    STARTUP_WARNINGS.append(msg)


def print_startup_warnings():
    for s in STARTUP_WARNINGS:
        logging.warning(s)
    STARTUP_WARNINGS.clear()
