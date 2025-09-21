# NezhaAgentHTTPBridge

轻量级桥接：接收 / 聚合上游 WebSocket 推送的服务器状态，缓存最近一条，提供 HTTP 接口 `/WeatherForecast/lastws` 供前端轮询；附带零依赖前端展示页 `UI.html`。

## 快速使用
1. 启动后端（恢复依赖并运行）。
2. 实现或接入 WebSocket 接收逻辑，将最新监控 JSON 写入 `WebSocketMessageStore`。
3. 部署/打开 `UI.html`，把其中 `CFG.api` 改为后端实际地址。
4. 前端开始轮询显示节点状态。

## 接口
- 路径：`/WeatherForecast/lastws`
- 作用：返回最近一次 WebSocket 缓存原始 JSON；无数据返回 404。
- 支持单对象 (`state + host`) 或外层含 `servers` / `list` / `data` 的数组结构。

## 使用时注意
- 地址同步：更改路由或放置到子路径时需同步修改前端 `CFG.api`。
- 地址修改：请修改`WebSocketClientBackgroundService`中的 WebSocket 地址。
- HTTPS：页面为 https 必须使用 https 接口，避免浏览器拦截。
- CORS：跨域部署需在后端显式放行来源，勿无条件全开放。
- 离线判定：依赖 `last_active` 与 `offlineSec`；时间戳用标准 ISO8601 / UTC。
- CPU 数值：前端假定范围 0–100；若原始为 0–1 需先换算。
- 字段完整：缺失 `state.*` 或 `host.*` 将触发“部分数据异常”提示。
- 轮询频率：默认 3000ms；节点多或接口压力大时增加间隔（建议 ≥2000ms）。
- 安全控制：必要时添加反向代理限制 / Header Token / 内网访问策略。
- 敏感数据：若包含内部 IP / 拓扑，不建议直接公网暴露。
- 扩展方向：
  - 多节点：使用字典按唯一 ID 覆盖最近状态
  - 历史曲线：落盘或环形缓冲存储时间序列
  - Prometheus：转换字段导出指标
  - 拉取转推：可改用 SSE / WebSocket 下行减少轮询
  - 字段映射：增加统一规范层，兼容不同上游格式

## 常见问题
| 现象 | 处理 |
|------|------|
| 404 | 尚未写入任何 WebSocket 数据或路由不正确 |
| “暂无数据” | 检查缓存写入逻辑与接口返回体 |
| CORS 报错 | 后端未放行对应 Origin |
| 离线误判 | 时间格式不可解析或服务器时间漂移 |
| CPU/内存显示异常 | 未统一单位/百分比换算 |

## 许可
本项目使用 Unlicense（公共领域 / 无限制），详见 `LICENSE` 文件。

## 致谢
哪吒探针及相关开源生态。