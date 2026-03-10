import time
import os
import subprocess
import sys

# Configuration
WATCH_DIR = os.path.dirname(os.path.abspath(__file__))
GENERATOR_SCRIPT = os.path.join(WATCH_DIR, "generate_dashboard.py")

def get_md_snapshot():
    """Returns a dictionary of filename -> mtime for all .md files."""
    snapshot = {}
    for root, dirs, files in os.walk(WATCH_DIR):
        for file in files:
            if file.endswith(".md"):
                full_path = os.path.join(root, file)
                try:
                    snapshot[full_path] = os.path.getmtime(full_path)
                except OSError:
                    pass
    return snapshot

def main():
    print(f"==================================================")
    print(f"   FishingGame Doc Watcher Started")
    print(f"   Watching: {WATCH_DIR}")
    print(f"   Target:   generate_dashboard.py")
    print(f"==================================================")
    
    last_snapshot = get_md_snapshot()
    
    try:
        while True:
            time.sleep(2) # Check every 2 seconds
            
            current_snapshot = get_md_snapshot()
            
            needs_update = False
            
            # Check for modified or new files
            for file, mtime in current_snapshot.items():
                if file not in last_snapshot:
                    print(f"[Change Detected] New file: {os.path.basename(file)}")
                    needs_update = True
                    break
                elif mtime != last_snapshot[file]:
                    print(f"[Change Detected] Modified: {os.path.basename(file)}")
                    needs_update = True
                    break
            
            # Check for deleted files
            if not needs_update and len(current_snapshot) != len(last_snapshot):
                print(f"[Change Detected] File deleted.")
                needs_update = True

            if needs_update:
                print(">> Regenerating Dashboard...", end=" ")
                # Run the generator
                result = subprocess.run([sys.executable, GENERATOR_SCRIPT], capture_output=True, text=True)
                if result.returncode == 0:
                    print("SUCCESS!")
                else:
                    print("FAILED!")
                    print(result.stderr)
                
                last_snapshot = current_snapshot
                print("--------------------------------------------------")

    except KeyboardInterrupt:
        print("\nWatcher stopped by user.")

if __name__ == "__main__":
    main()

