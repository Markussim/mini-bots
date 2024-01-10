local mytable = {}
mytable["testkey"] = "testvalue"

local subtable = {}
subtable["subkey"] = "subvalue"

mytable["2dtest"] = subtable

local json = jsonManager:TableToJson(mytable)
local newtable = jsonManager:JsonToTable(json)

return newtable["subkey"]["subvalue"]
