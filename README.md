# Monaco Editor Verification

This document provides instructions on how to run the Playwright script to verify the Monaco editor integration.

## Prerequisites

1.  **Python:** Ensure you have Python installed.
2.  **Playwright:** Install Playwright and its browser dependencies:
    ```bash
    pip install playwright
    playwright install
    ```

## Running the Verification Script

1.  **Disable Authentication:** Before running the application, you need to disable authentication. To do this, comment out the authentication-related services in `InstanceManager.Host.WA/Program.cs` and `InstanceManager.Host.WA/App.razor` as shown in the pull request.

2.  **Run the Blazor Application:**
    ```bash
    /home/jules/.dotnet/dotnet run --project InstanceManager.Host.WA
    ```

3.  **Run the Verification Script:**
    ```bash
    python scripts/verify_monaco.py
    ```

The script will launch a browser, navigate to the application, click the "test btn", and take a screenshot of the Monaco editor. The screenshot will be saved as `monaco_editor.png` in the `/home/jules/verification` directory.
