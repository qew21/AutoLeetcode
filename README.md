# Autoleetcode


#### 介绍
使用selenium自动读取Leetcode题目，交给不同的大模型处理，自动提交统计正确率

目前看顶尖的大模型尚不能高正确率的通过中等难度题目，预期通过微调AlchemistCoder提高准确率

#### 使用说明
1. 下载匹配Chrome浏览器版本的[driver](https://googlechromelabs.github.io/chrome-for-testing/#stable)
2. 将driver放到当前目录下
3. 参考如下配置修改配置中的ApiKey和BaseUrl等参数
4. 安装依赖，运行 ```python main.py```
5. 处于登录状态时，会自动跳转到登录页面，登录成功后会自动跳转到题目页面
```json5
{
    "BaseUrl": "https://dashscope.aliyuncs.com/compatible-mode/v1",  // dashscope api url
    "ApiKey": "sk-****",  // dashscope api key
    "Model": "qwen2-72b-instruct",  // model name
    "Level": "hard",  // difficulty level
    "Skip": false,  // skip already solved problems
    "Count": 100,  // problem count
    "StartPage": 1  // start page
}
```

#### 示例

![demo](demo.jpg)

---
#### Introduction
Automatically read Leetcode problems using Selenium, hand them over to different large models for processing, and automatically submit solutions while tracking accuracy rates.

It appears that even top-tier large models struggle to achieve high accuracy on medium difficulty problems. The expectation is to improve accuracy by fine-tuning the AlchemistCoder model.

#### Instructions
1. Download the driver [here](https://googlechromelabs.github.io/chrome-for-testing/#stable) that matches your Chrome browser version.
2. Place the driver in the current directory.
3. Create a `config.json` file based on example below, modifying parameters such as ApiKey and BaseUrl.
4. Install requirements and run ```python main.py```
5. When in a logged-out state, the system will automatically redirect to the login page; after successful login, it will automatically navigate to the problem page.


