
from playwright.sync_api import Page, expect, sync_playwright

def verify_monaco_editor(page: Page):
    """
    This test verifies that the Monaco editor is displayed on the homepage
    and that it is populated with data when the 'test btn' is clicked.
    """
    # 1. Arrange: Go to the application homepage.
    page.goto("http://localhost:5070")

    # 2. Act: Find the "test btn" and click it.
    test_button = page.get_by_role("button", name="test btn")
    test_button.click()

    # Wait for the editor to be populated
    page.wait_for_timeout(1000)

    # 3. Screenshot: Capture the final result for visual verification.
    page.screenshot(path="/home/jules/verification/monaco_editor.png")

if __name__ == "__main__":
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()
        try:
            verify_monaco_editor(page)
        finally:
            browser.close()
