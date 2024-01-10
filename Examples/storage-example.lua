local count = tonumber(storageManager:GetData())
if count == "" or count == nil then
 count = 0
end

count = count + 1

storageManager:ReplaceData(tostring(count))

if math.fmod(count, 10) == 0 then
  return "Congrats, this is the " .. count .. "th message"
end
