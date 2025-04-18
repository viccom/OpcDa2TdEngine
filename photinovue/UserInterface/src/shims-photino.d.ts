// 扩展 Photino 的 window.external 类型
interface Window {
  external: {
    sendMessage: (message: string) => void;
    receiveMessage: (callback: (message: string) => void) => void;
  };
}
