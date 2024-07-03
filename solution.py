import json
import re

from openai import OpenAI
from config import config
from loguru import logger
from json_repair import repair_json


def parse_json(content: str) -> dict:
    try:
        pattern = r"(?<=\`\`\`json)[\s\S]*?(?=\`\`\`)"
        matches = re.findall(pattern, content, re.DOTALL)
        if matches:
            result = matches[0]
            result = repair_json(result)
            return json.loads(result)
    except Exception as e:
        logger.error(e)
    return {}


def get_response(title, problem, code) -> dict:
    try:
        prompt = (f"{title} {problem}, Please answer using Python 3, including the given preamble code {code}，"
                  f"The example format should be```json\r\n{{\r\n  \"abstract\": \"使用动态规划和最小堆"
                  f"...\",\r\n  \"solution\": \"\\nclass Solution:\\n   ...\\n\"\r\n}}\r\n```， "
                  f"This includes two keys, 'abstract' for a description of the method, and 'solution' for the pure "
                  f"Python code answer, which will be submitted to LeetCode. Please double-check to ensure the result "
                  f"is correct, and only one answer is needed.")
        client = OpenAI(
            api_key=config.ApiKey,
            base_url=config.BaseUrl,
        )
        completion = client.chat.completions.create(
            model=config.Model,
            messages=[{'role': 'system', 'content': 'You are a senior python developer.'},
                      {'role': 'user', 'content': prompt}]
        )
        logger.debug(completion.model_dump_json())
        result = completion.choices[0].message.content
        return parse_json(result)
    except Exception as e:
        logger.error(e)
        return {}
