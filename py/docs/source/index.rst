======================
Selenium Client Driver
======================

Introduction
============

Python language bindings for Selenium WebDriver.

The `selenium` package is used to automate web browser interaction from Python.

+-----------------+--------------------------------------------------------------------------------------+
| **Home**:       | https://selenium.dev                                                                 |
+-----------------+--------------------------------------------------------------------------------------+
| **GitHub**:     | https://github.com/SeleniumHQ/Selenium                                               |
+-----------------+--------------------------------------------------------------------------------------+
| **PyPI**:       | https://pypi.org/project/selenium                                                    |
+-----------------+--------------------------------------------------------------------------------------+
| **API Docs**:   | https://selenium.dev/selenium/docs/api/py/api.html                                   |
+-----------------+--------------------------------------------------------------------------------------+
| **IRC/Slack**:  | `Selenium chat room <https://www.selenium.dev/support/#ChatRoom>`_                   |
+-----------------+--------------------------------------------------------------------------------------+

Several browsers/drivers are supported (Firefox, Chrome, Edge, Safari), as well as the Remote protocol.

Supported Python Versions
=========================

* Python 3.9+

Installing
==========

If you have `pip <https://pip.pypa.io/>`_ on your system, you can simply install or upgrade the Python bindings::

    pip install -U selenium

You may want to consider using a `virtual environment <https://packaging.python.org/en/latest/guides/installing-using-pip-and-virtual-environments>`_
to create isolated Python environments.

Drivers
=======

Selenium requires a driver to interface with the chosen browser (chromedriver, edgedriver, geckodriver, etc).

In older versions of Selenium, it was necessary to install and manage these drivers yourself. You had to make sure the driver
executable was available on your system `PATH`, or specified explicitly in code. Modern versions of Selenium handle browser and
driver installation for you with `Selenium Manager <https://www.selenium.dev/documentation/selenium_manager>`_. You generally
don't have to worry about driver installation or configuration now that it's done for you when you instantiate a WebDriver.
Selenium Manager works with most supported platforms and browsers. If it doesn't meet your needs, you can still install and
specify browsers and drivers yourself.

Links to some of the more popular browser drivers:

+--------------+-----------------------------------------------------------------------+
| **Chrome**:  | https://chromedriver.chromium.org/downloads                           |
+--------------+-----------------------------------------------------------------------+
| **Edge**:    | https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/ |
+--------------+-----------------------------------------------------------------------+
| **Firefox**: | https://github.com/mozilla/geckodriver/releases                       |
+--------------+-----------------------------------------------------------------------+
| **Safari**:  | https://webkit.org/blog/6900/webdriver-support-in-safari-10/          |
+--------------+-----------------------------------------------------------------------+

Example 0:
==========

* launch a new Chrome browser
* load a web page
* close the browser

.. code-block:: python

    from selenium import webdriver


    driver = webdriver.Chrome()
    driver.get('https://selenium.dev/')
    driver.quit()

Example 1:
==========

* launch a new Chrome browser
* load the Selenium documentation page
* find the "Webdriver" link
* click the "WebDriver" link
* close the browser

.. code-block:: python

    from selenium import webdriver
    from selenium.webdriver.common.by import By


    driver = webdriver.Chrome()

    driver.get('https://selenium.dev/documentation')
    assert 'Selenium' in driver.title

    elem = driver.find_element(By.ID, 'm-documentationwebdriver')
    elem.click()
    assert 'WebDriver' in driver.title

    driver.quit()

Example 2:
==========

Selenium WebDriver is often used as a basis for testing web applications. Here is a simple example using Python's standard `unittest <http://docs.python.org/3/library/unittest.html>`_ library:

.. code-block:: python

    import unittest
    from selenium import webdriver


    class GoogleTestCase(unittest.TestCase):

        def setUp(self):
            self.driver = webdriver.Firefox()
            self.addCleanup(self.driver.quit)

        def test_page_title(self):
            self.driver.get('https://www.google.com')
            self.assertIn('Google', self.driver.title)

    if __name__ == '__main__':
        unittest.main(verbosity=2)

Selenium Grid (optional)
==========================

For local Selenium scripts, the Java server is not needed.

To use Selenium remotely, you need to also run the Selenium grid.
For information on running Selenium Grid: https://www.selenium.dev/documentation/grid/getting_started/

To use Remote WebDriver see: https://www.selenium.dev/documentation/webdriver/drivers/remote_webdriver/?tab=python

Use The Source Luke!
====================

View source code online:

+-----------+------------------------------------------------------+
| Official: | https://github.com/SeleniumHQ/selenium/tree/trunk/py |
+-----------+------------------------------------------------------+

Contributing
=============

 - Create a branch for your work
 - Ensure `tox` is installed (using a `virtualenv` is recommended)
 - Run: `python -m venv venv && source venv/bin/activate && pip install tox`
 - After making changes, before committing execute `tox -e linting`
 - If tox exits `0`, commit and push. Otherwise fix the newly introduced style violations.
 - `flake8` requires manual fixes
 - `black` will rewrite the violations automatically, however the files are unstaged and should staged again.
 - `isort` will rewrite the violations automatically, however the files are unstaged and should staged again.
