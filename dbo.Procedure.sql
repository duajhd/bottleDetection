


CREATE PROCEDURE selecname
@Pname varchar(20)
as 
select id as 手机品牌,name as 手机型号 from test where name=@Pname




CREATE PROCEDURE selecname @Pname varchar(20) as  select id as 手机品牌,name as 手机型号 from test where name=@Pname return 0


