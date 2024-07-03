import datetime
import json
import os.path
import re
import time

from loguru import logger
from selenium import webdriver
from selenium.webdriver import ActionChains
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import pyperclip
from selenium.webdriver.common.keys import Keys
from config import config
from solution import get_response

logger.add('app.log', format="{time} {level} {message}", level="DEBUG")


def init_chrome_driver() -> webdriver.Chrome:
    # check chrome driver version match Chrome browser version
    option = webdriver.ChromeOptions()
    option.add_experimental_option("detach", True)
    option.add_argument(f"--user-data-dir={os.path.abspath('cache')}")  # 替换为你的路径
    service = webdriver.ChromeService(executable_path='./chromedriver.exe')
    driver = webdriver.Chrome(options=option, service=service)
    return driver


def choose_python3(driver):
    try:
        language_path = "//div[starts-with(@id, 'headlessui-popover-button-:r1')]"
        WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.XPATH, language_path)))
        language = driver.find_element(by=By.XPATH, value=language_path)
        if language.text != 'Python3':
            language.click()
            python3 = language.find_element(By.XPATH, ".//descendant::*/text()[.='Python3']/parent::*")
            python3.click()
    except Exception as e:
        logger.error(e)


def solve(driver, problem_url: str) -> dict:
    result = {}
    driver.get(problem_url)
    choose_python3(driver)
    title = driver.find_element(value='text-title-large', by=By.CLASS_NAME).text
    problem = driver.find_element(value='elfjS', by=By.CLASS_NAME).text
    _id = re.search(r"^\d+", title).group()
    result['id'] = _id
    result['title'] = title
    result['problem'] = problem
    try:
        code_area = (driver.find_element(value='editor', by=By.ID)
                     .find_element(by=By.CLASS_NAME, value='monaco-mouse-cursor-text'))
        logger.debug(code_area.text)
        response = get_response(title, problem, code_area.text)
        if response:
            logger.debug(response)
            result.update(response)
            solution = response['solution']
            actions = ActionChains(driver)
            actions.move_to_element(code_area).click()
            pyperclip.copy(solution)
            actions.key_down(Keys.CONTROL).send_keys('a').key_up(Keys.CONTROL)
            actions.send_keys(Keys.DELETE)
            actions.key_down(Keys.CONTROL).send_keys('v').key_up(Keys.CONTROL)

            actions.perform()
            time.sleep(3)

            # run code
            element = driver.find_element(By.CSS_SELECTOR, '[data-e2e-locator="console-run-button"]')
            element.click()

            # wait for result
            green_element_located = EC.presence_of_element_located((By.CSS_SELECTOR, '.rounded-full.bg-red-s'))
            red_element_located = EC.presence_of_element_located((By.CSS_SELECTOR, '.rounded-full.bg-green-s'))
            element_located = EC.any_of(green_element_located, red_element_located)
            WebDriverWait(driver, 60).until(element_located)
            green = len(driver.find_elements(By.CSS_SELECTOR, '.rounded-full.bg-green-s'))
            red = len(driver.find_elements(By.CSS_SELECTOR, '.rounded-full.bg-red-s'))
            element = driver.find_element(By.CSS_SELECTOR, '[data-e2e-locator="console-submit-button"]')
            element.click()
            result['status'] = None
            # submit when no error occurs
            if green > 0 and red == 0:
                status = driver.find_element(By.CSS_SELECTOR, '.text-green-s')
                result['status'] = status.text
    except Exception as e:
        logger.error(e)
    return result


def check_login(driver):
    try:
        login_button = driver.find_element(By.ID, 'navbar_sign_in_button')
        if login_button:
            url = 'https://leetcode.cn/accounts/login/?next=%2F'
            driver.get(url)
            logger.debug('Wait for login')
            WebDriverWait(driver, 300).until(
                EC.url_changes(url)
            )
            logger.debug('Login success')
    except:
        logger.debug('Already login')


def main():
    driver = init_chrome_driver()

    filename = datetime.datetime.now().strftime('%Y_%m_%d_%H_%M_%S') + '.jsonl'
    logger.debug(filename)
    cnt = config.Count
    try:
        while cnt > 0:
            url = f"https://leetcode.cn/problemset/?page={config.StartPage}"
            driver.get(url)
            time.sleep(5)
            if cnt == config.Count:
                check_login(driver)
            problems = driver.find_elements(By.CSS_SELECTOR, "[role='rowgroup']")[2].find_elements(By.XPATH, "./*")
            data = []
            for i, problem in enumerate(problems):
                if not problem:
                    continue

                children = problem.find_elements(By.XPATH, "./*")
                difficulty = children[4].text.strip()
                tag = children[0].find_elements(By.XPATH, "./*")
                link = problem.find_element(By.XPATH, ".//a").get_attribute("href")
                logger.debug(f"{i + 1} {difficulty} {tag} {link}")
                if config.Skip and len(tag):
                    continue
                if 'envId' in link:
                    continue
                if config.Level != 'hard' and difficulty == '困难':
                    continue
                if config.Level == 'easy' and difficulty == '中等':
                    continue
                data.append((link, difficulty))
            for link, difficulty in data:
                cnt -= 1
                result = solve(driver, link)
                result['link'] = link
                result['difficulty'] = difficulty
                if result:
                    with open(filename, 'a') as f:
                        f.write(json.dumps(result, ensure_ascii=False) + '\n')
            config.StartPage += 1
    except Exception as e:
        logger.error(e)
    finally:
        driver.quit()


if __name__ == '__main__':
    main()
