# 打印服务Node

##### 此服务必须与Easy_OA后端的print模块搭配使用，该解决方案包含printNode：打印服务，PrintToImage：pdf转缩略图服务，PrintTOPDFNode:任意文件转pdf服务。

下面是大概的联系图，打印节点服务与主后端服务直接通过consul和rocketmq队列联系，双方互为消费者也互为生产者,整个打印流程都是异步的。

![image-20231117151418065](readme.png)



### 此项目无法独立使用，需要使用得研究传输类，自己完善主后端，并调好中间件，依赖.net7.0和c#还有adobe DC PRO 还有微软Office 2016以上版本。

目前仅兼容brother打印机，其余品牌可以稍作修改即可兼容，理论来说大众配置都可兼容!