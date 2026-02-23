# Script to patch tqdm std.py
import sys

tqdm_path = r"C:\Program Files\AILocal\ComfyUI\venv\Lib\site-packages\tqdm\std.py"

with open(tqdm_path, 'r', encoding='utf-8') as f:
    content = f.read()

# The original code
old_code = '''        if fp in (sys.stderr, sys.stdout):
            getattr(sys.stderr, 'flush', lambda: None)()
            getattr(sys.stdout, 'flush', lambda: None)()'''

# The patched code
new_code = '''        if fp in (sys.stderr, sys.stdout):
            try:
                getattr(sys.stderr, 'flush', lambda: None)()
            except (OSError, ValueError, IOError):
                pass
            try:
                getattr(sys.stdout, 'flush', lambda: None)()
            except (OSError, ValueError, IOError):
                pass'''

if old_code in content:
    content = content.replace(old_code, new_code)
    with open(tqdm_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Patched successfully!")
elif "try:" in content and "getattr(sys.stderr, 'flush'" in content:
    print("Already patched!")
else:
    print("Could not find code to patch!")
    print("Looking for pattern...")
    if "getattr(sys.stderr, 'flush', lambda: None)()" in content:
        print("Found the flush call but pattern didn't match exactly")
