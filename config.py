import json
from collections import namedtuple

# 读取json文件
with open('config.json', 'r') as f:
    config_dict = json.load(f)

# 将字典转换为named tuple
Config = namedtuple('Config', config_dict.keys())
config = Config(**config_dict)