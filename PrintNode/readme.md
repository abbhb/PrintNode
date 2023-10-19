# 打印节点

## 此节点依赖adobe软件，需要win 64位，且安装了Adobe Acrobat DC 2022.001版本

## 依赖主服务和consul以及rocketmq队列

### 运行原理：
#### 主服务用户点击开始打印，主服务发相关参数到rocketmq打印队列，并且带了指定打印机的tag，根据tag来确定是哪个打印机消费

#### 程序运行时会自动注册接口到consul，并且注册tag，便于主服务器选择打印机


```
开放api：
http://192.168.12.122:23123/api/health 健康检查
http://192.168.12.122:23123/api/printDevice

反正就是Controller的名字首字母变小写，其余不变
http://192.168.12.122:23123/api/printDevice/status
Delete http://192.168.12.122:23123/api/printDevice/cancel/{id} 
```