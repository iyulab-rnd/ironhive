export interface MessageContentBase {
  id?: string;
  index?: number;
}

export interface TextContent extends MessageContentBase {
  type: "text";
  value?: string;
}

export interface ImageContent extends MessageContentBase {
  type: "image";
  data?: string;
}

export type ToolStatus = "pending" | "running" | "completed" | "failed";

export interface ToolContent extends MessageContentBase {
  type: "tool";
  status?: ToolStatus;
  name?: string;
  arguments?: any;
  result?: any;
}

export type MessageContent = TextContent | ImageContent | ToolContent;

export interface Message {
  role: "user" | "assistant";
  avatar?: string;
  name?: string;
  content?: MessageContent[];
  timestamp?: string;
}
