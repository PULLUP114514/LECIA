# LECIA
LECIA Enables Class Island Anywhere!

一个简单的小插件 :D

huhhhh... 具体能干什么，建议搭配单片机食用 (比如桌搭啥的🤔)

**注意事项：**

- 配置文件使用INI格式存储(因为“{}”是JSON的关键字  :-P )

- 向串口发送的数据会在末尾添加换行符"\n"

- 未来如果心情好会添加对 UDP/TCP 的支持 :)

- 遇到问题扔ISSUE

- 作者6月之前都忙，有能力&有需要的功能可以自行PR :D

## 2. 配置设置
### 2.1 配置中的关键词和示例

> [!important]
> 关键词是大小写敏感的！！！！

以下为关键词和对应的释义：

    {NextPointTime}              距离下个时间点的剩余时间（可能为00:00:00.0）
    {ClassLeftTime}              距离上课的时间（可能为00:00:00.0）
    {BreakingLeftTime}           距离下课的时间（可能为00:00:00.0）
    {CurrentSubjectName}         当前的课程名  （当无课程时为“无课程”）
    {CurrentClassPlan}           当前的课表    （当此处为空时显示“noclass”）

以下为使用示例：

    NextPointTime: {NextPointTime}    ClassLeftTime: {ClassLeftTime}    BreakingLeftTime: {BreakingLeftTime}    CurrentSubjectName:{CurrentSubjectName}    CurrentClassPlan: {CurrentClassPlan}

这是上面的示例的输出：

    NextPointTime: 00:15:10.3002447    ClassLeftTime: 00:00:00    BreakingLeftTime: 00:15:10.3002447    CurrentSubjectName:数学    CurrentClassPlan: 语文,语文,语文,语文,语文,语文,语文,语文,语文,语文,语文,语文,数学,数学,数学,数学,数学,数学,政治,政治


### 2.2 配置文件

配置文件存储在:

    %ClassIsland_BinaryDirectory%\Config\Plugins\LECIA\config.ini

示例:
 
    [mainconfig]
    autostart=1
    datatarget=0
    comport=COM6
    baundrate=115200
    maindataformat=NextPointTime: {NextPointTime}    ClassLeftTime: {ClassLeftTime}    BreakingLeftTime: {BreakingLeftTime}    CurrentSubjectName:{CurrentSubjectName}    CurrentClassPlan: {CurrentClassPlan}

解释：

项| 释义
--------|---
autostart|1 -> 在CI启动时启用   0->不在CI启动时启用
datatarget|0 -> 输出到COM
comport|串口编号
baundrate|波特率（推荐115200）
maindataformat|见 "2.1 配置中的关键词和示例"
