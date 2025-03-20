# TCM成分工具

从TCMID和TCMSP等数据库扒了数据，然后写了一些小工具进行整理和查询



## tcm-component-dump

从TCMID和TCMSP的网页中整理信息到json



## tcm-component-smiles

将InChiKey转化到SMILES结构式的工作流程



## tcm-component-query

数据查询

1. 检查药物名称
2. 获取InChiKey标识符和SMILES结构式
3. 把分子对接等得到的阳性InChiKey标识符转换到药材和分子名



## tcm-component-SugarRemoval

中药成分中有不少糖苷类化合物，一般在消化吸收过程中会把糖苷切掉并在体内以苷元的形式发挥功能。

因此在对接前最好用 https://github.com/JonasSchaub/SugarRemoval 把相关分子还原成苷元



