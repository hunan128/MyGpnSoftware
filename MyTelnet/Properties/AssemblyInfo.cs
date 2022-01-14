﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的常规信息通过下列属性集
// 控制。更改这些属性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("排故好帮手")]
[assembly: AssemblyDescription(""
     + "2022.01.13 支持新7618-c的FPGA版本检查" + "\r\n"
     + "2022.01.06 支持新7618-c的FPGA版本升级" + "\r\n"
    + "2021.10.15 升级支持7615文件" + "\r\n"
    + "2021.08.04 新增CPLD / VOSS / REBOOTOS文件升级" + "\r\n"
+ "2021.07.31 新增OTN透传业务故障定位" + "\r\n"
+ "2021.07.25 新增OTN分组业务故障定位" + "\r\n"
+ "2021.07.05 新增EOS业务故障定位" + "\r\n"
+ "2021.06.24 新增了文件刷新功能" + "\r\n"
+ "2021.03.27 新增了Debug APP版本检查" + "\r\n"
+ "2021.03.25 新增了SDN文件升级" + "\r\n"
+ "2021.03.11 新增了7611 fpga升级" + "\r\n"
+ "2020.12.30 新增了移动集采版本升级和板卡改制" + "\r\n"
+ "2020.09.10 提升了VNC远程控制桌面的安全性" + "\r\n"
+ "2020.08.28 新增了ip或者域名ping检测功能" + "\r\n"
    + "2020.08.26 新增了软件下载功能（绿色版下载）" + "\r\n"
    + "2020.08.20 修改了远程共享设计，端口随机生成，自动获取用户" + "\r\n"
    + "2020.07.24 新增SNC-S的TCM时隙告警查询" + "\r\n"
    + "2020.07.23 解决了EOS、OTN排查故障需要点击俩次问题，支持一次准确定位"+"\r\n"
    + "2020.07.20 解决了EOS故障排出现“值对于Int32太大或太小”的提示" + "\r\n"
    + "2020.07.16 新增OTN板卡查询fpga功能，解决了och保护查询的一些bug" + "\r\n"
    + "2020.07.16 解决了上传config文件不会换行问题" + "\r\n"
    + "2020.07.10 支持OTN一键排查问题增加底层配置检查和寄存器配置检查" + "\r\n"
    + "2020.07.07 支持VNC+FRPC内网桌面共享功能" + "\r\n"
    + "2020.07.04 解决了批量升级的一些BUG" + "\r\n"
    + "2020.07.01 增加GPN设备批量下载和上传功能" + "\r\n"
    + "2020.06.22 新增Mib节点在线查询功能" + "\r\n"
    + "2020.06.13 新增Try命令完善下载上传EOS排故OTN排故检查文件连接软件卡死" + "\r\n"
    + "2020.06.11 新增SNMP/MIB节点查询功能，支持Trap监听功能" + "\r\n"
    + "2020.06.05 新增CPU、内存、温度实时状态显示" + "\r\n"
    + "2020.05.30 优化升级和上传备份的代码，更稳定" + "\r\n"
    + "2020.05.28 新增问题日志自动记录功能和问题反馈窗口" + "\r\n"
    + "2020.05.25 新增760A的fpga下载功能" + "\r\n"
    + "2020.05.25 新增支持OTN大包的下载和上传功能" + "\r\n"
    + "2020.05.21 新增日志邮件发送功能" + "\r\n"
    + "2020.05.20 新增OTN板卡故障排查功能" + "\r\n"
    + "2020.04.16 新增上传备份功能，支持所有复选框内容灵活备份" + "\r\n"
    + "2020.04.12 新增设备与下载文件对比功能，提升升级成功概率为99.99%" + "\r\n"
    + "2020.04.09 新增GPN7600EMS模块安装卸载功能" + "\r\n"
    + "2020.04.07 新增OTN板卡改制功能" + "\r\n"
    + "2020.04.07 解决了OTNfpga双主控不能升级成功的BUG" + "\r\n"
    + "2019.12.30 新增命令行循环打印功能和设备ip地址可选功能" + "\r\n"
    + "2019.12.11 新增一键排查故障功能和一键导出日志功能" + "\r\n"
    + "2019.11.21 新增sysfile下载功能" + "\r\n"
    + "2019.11.21 新增一键导出设备信息和日志功能" + "\r\n"
    + "2019.11.11 新增OTN设备升级和自定义升级文件" + "\r\n"
    + "2019.05.24 支持NMS-V2板卡升级" + "\r\n"
    + "2018.07.09 支持批量测试设备在线功能" + "\r\n"
    + "2018.06.26 支持720-U1-2批量升级功能" + "\r\n"
    + "2018.06.20 支持批量备份数据库功能" + "\r\n"
    + "2018.06.19 支持批量保存功能" + "\r\n"
    + "2018.06.08 一键升级只有GPN7600可以使用" )]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("排故好帮手")]
[assembly: AssemblyProduct("排故好帮手")]
[assembly: AssemblyCopyright("版权所有 (C) 胡楠 2018.06.08")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 属性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("e1b39a5d-ba7a-4d8e-8c92-9f1d206f2ce9")]

// 程序集的版本信息由下面四个值组成:
//
//      主版本
//      次版本 
//      内部版本号
//      修订号
//
[assembly: AssemblyVersion("2022.1.6.5")]
