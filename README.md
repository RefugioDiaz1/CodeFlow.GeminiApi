# CodeFlow.GeminiApi

API backend desarrollada en **ASP.NET Core** que actúa como intermediaria segura entre la aplicación **WhaAuto** (chatbot) y la API de **Google Gemini**, centralizando la lógica de integración, seguridad y procesamiento de respuestas de IA.

---

## 🧠 Contexto del proyecto

**WhaAuto** es una aplicación tipo **chatbot** que permite interactuar con capacidades de inteligencia artificial mediante mensajes automatizados.  
Actualmente, la aplicación cliente se encuentra disponible **únicamente en Android**.

Para evitar exponer credenciales sensibles y desacoplar la lógica de IA del cliente, se desarrolló **CodeFlow.GeminiApi** como una API intermediaria.

---

## 🎯 Rol de CodeFlow.GeminiApi

Esta API funciona como una **capa intermedia (Gateway / Facade)** entre WhaAuto y Google Gemini:


La aplicación cliente **nunca se comunica directamente con Gemini**, lo que permite mayor seguridad, control y escalabilidad.

---

## ⚙️ Funcionalidades principales

- 🔐 **Gestión segura de la API Key**
  - Las credenciales de Google Gemini se manejan exclusivamente en el backend.
  - Nunca se exponen en la aplicación Android.

- 🤖 **Integración con Google Gemini**
  - Envío de prompts desde el chatbot.
  - Recepción y procesamiento de respuestas de IA.

- 📦 **API intermediaria**
  - Centraliza la lógica de comunicación con Gemini.
  - Permite modificar o extender la integración sin afectar al cliente.

- 🧱 **Desacoplamiento del cliente**
  - WhaAuto solo consume endpoints propios.
  - La lógica de IA puede evolucionar sin cambios en la app Android.

- 🚀 **Preparada para crecimiento**
  - Posibilidad de agregar validaciones, logging, rate limiting, cache o autenticación.
  - Soporte futuro para otros clientes (web, iOS, otros servicios).

---

## 🏗️ Arquitectura

- **Cliente (WhaAuto - Android)**
  - Chatbot que envía mensajes a la API.
- **CodeFlow.GeminiApi**
  - Orquesta solicitudes.
  - Maneja seguridad y configuración.
  - Procesa respuestas.
- **Google Gemini API**
  - Motor de inteligencia artificial.

Este enfoque sigue buenas prácticas de **arquitectura API-first** y separación de responsabilidades.

---

## 🧑‍💻 Stack tecnológico

- ASP.NET Core
- C#
- API REST
- HTTP Client
- Manejo seguro de configuración y secretos

---

## 🔐 Seguridad

> La API Key de Google Gemini se almacena y utiliza únicamente en el backend, evitando su exposición en aplicaciones cliente o repositorios públicos.

Este diseño reduce riesgos de:
- Filtración de credenciales
- Uso indebido de la API
- Costos no controlados

---

## 📌 Estado actual

- ✅ Integración funcional con Google Gemini
- ✅ Comunicación estable con WhaAuto (Android)
- 🔄 Evolución continua del backend
- 📱 Soporte actual: **Android**
- 🌐 Soporte futuro: Web / iOS (planeado)

---

## 🧠 Cómo explicarlo en entrevista

> “Desarrollé una API en .NET que actúa como intermediaria entre un chatbot Android y Google Gemini, protegiendo las credenciales, centralizando la lógica de IA y permitiendo escalar la solución sin acoplar el cliente.”

---

## 🚀 Posibles mejoras futuras

- Autenticación por cliente
- Rate limiting
- Cache de respuestas
- Logging estructurado
- Soporte multi-plataforma
- Versionado de endpoints

---

