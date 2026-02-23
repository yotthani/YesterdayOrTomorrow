# Delete all .pyc cache files in ComfyUI app folder
$appPath = "C:\Program Files\AILocal\ComfyUI\app"
$pycachePath = Join-Path $appPath "__pycache__"

if (Test-Path $pycachePath) {
    Remove-Item -Path $pycachePath -Recurse -Force
    Write-Host "Deleted __pycache__ folder: $pycachePath"
}

# Also check comfy folder for cached files
$comfyPycache = "C:\Program Files\AILocal\ComfyUI\comfy\__pycache__"
if (Test-Path $comfyPycache) {
    Remove-Item -Path $comfyPycache -Recurse -Force
    Write-Host "Deleted comfy __pycache__"
}

$kdiffPycache = "C:\Program Files\AILocal\ComfyUI\comfy\k_diffusion\__pycache__"
if (Test-Path $kdiffPycache) {
    Remove-Item -Path $kdiffPycache -Recurse -Force
    Write-Host "Deleted k_diffusion __pycache__"
}

# Write the corrected logger.py
$loggerContent = @'
from collections import deque
from datetime import datetime
import io
import logging
import sys
import threading

logs = None
stdout_interceptor = None
stderr_interceptor = None


class LogInterceptor(io.TextIOWrapper):
    def __init__(self, stream,  *args, **kwargs):
        buffer = stream.buffer
        encoding = stream.encoding
        super().__init__(buffer, *args, **kwargs, encoding=encoding, line_buffering=stream.line_buffering)
        self._lock = threading.Lock()
        self._flush_callbacks = []
        self._logs_since_flush = []

    def write(self, data):
        entry = {"t": datetime.now().isoformat(), "m": data}
        with self._lock:
            self._logs_since_flush.append(entry)

            # Simple handling for cr to overwrite the last output if it isnt a full line
            # else logs just get full of progress messages
            if isinstance(data, str) and data.startswith("\r") and logs and not logs[-1]["m"].endswith("\n"):
                logs.pop()
            if logs is not None:
                logs.append(entry)
        try:
            super().write(data)
        except (OSError, ValueError):
            pass

    def flush(self):
        try:
            super().flush()
        except (OSError, ValueError):
            pass
        for cb in self._flush_callbacks:
            try:
                cb(self._logs_since_flush)
            except:
                pass
            self._logs_since_flush = []

    def on_flush(self, callback):
        self._flush_callbacks.append(callback)


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

    # Override output streams and log to buffer
    logs = deque(maxlen=capacity)

    global stdout_interceptor
    global stderr_interceptor
    stdout_interceptor = sys.stdout = LogInterceptor(sys.stdout)
    stderr_interceptor = sys.stderr = LogInterceptor(sys.stderr)

    # Setup default global logger
    logger = logging.getLogger()
    logger.setLevel(log_level)

    stream_handler = logging.StreamHandler()
    stream_handler.setFormatter(logging.Formatter("%(message)s"))

    if use_stdout:
        # Only errors and critical to stderr
        stream_handler.addFilter(lambda record: not record.levelno < logging.ERROR)

        # Lesser to stdout
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
'@

$targetPath = "C:\Program Files\AILocal\ComfyUI\app\logger.py"
$loggerContent | Out-File -FilePath $targetPath -Encoding utf8NoBOM -Force
Write-Host "Patched $targetPath successfully (UTF-8 no BOM)!"

# Verify the patch
$content = Get-Content $targetPath -Raw
if ($content -match "except \(OSError, ValueError\)") {
    Write-Host "Verification: Patch applied correctly!"
} else {
    Write-Host "WARNING: Patch may not have been applied correctly"
}
