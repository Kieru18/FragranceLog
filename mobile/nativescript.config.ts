import { NativeScriptConfig } from '@nativescript/core';

export default {
  id: 'org.kieru.fragrancelog',
  appPath: 'src',
  appResourcesPath: 'App_Resources',
  android: {
    v8Flags: '--expose_gc',
    markingMode: 'none'
  }
} as NativeScriptConfig;
