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


## 添加打印机支持要求
可传参的exe程序 
- 命名要求: xxxSetting.exe 在运行本程序前的config.yaml里指定的driveName需要指定xxx
- ## 传参要求：

### 不算exe本身还要三个参数 
- 第一个参数为打印机在系统里的名称 方便程序找到要设置的打印机
- 第二个参数为int  1为单面 2双面 3双面上翻（2，3需要打印机本身支持，结合实际传参）
- 第三个参数为int 1为纵向 2为横向

具体什么代码写出的exe无所谓，能直接终端通过`xxxSetting.exe "2240L-D" 1 1`这种命令直接执行都行，xxx由yaml配置，程序扔到本节点服务的根目录即可！！！

## yaml配置文件最少包含:
### (`accesskey,secretkey`指的是`rocketmq`的消息队列如果没设置就默认置空就行) `topic`和`rtopic`必须一样 `ip`取决于`rocketmq`的服务器的namesrv地址
```yaml
ip: 192.168.12.12:9876
topic: print_send_msg
rtopic: print_send_msg_r
tag: ceshi_1 #tag必须唯一
name: 测试电脑
accesskey: 
secretkey: 
myip: 192.168.12.36 #插上打印机设备的本机ip,或者时域名，能访问本机的任意地址，不带http，统一目前使用http
myport: 23123 #插上打印机设备本机运行时想要暴露的端口，任意一个没冲突的会自动注册到服务中心
consulip: 192.168.12.12
consulport: 8500
driveName: 2240-Dl #打印机在系统设置里的名字
exeName: BrotherSetting.exe
```


### 一切配置好后运行PrintNode.exe就行
#注意：同一个打印机只允许一次程序，建议一个打印机就一个id只运行一个程序，否则可能出问题


# 依赖
- adobe dc软件
- XXXSetting的exe