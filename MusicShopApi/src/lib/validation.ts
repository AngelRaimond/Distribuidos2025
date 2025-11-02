import Joi from 'joi';

export const instrumentSchema = Joi.object({
  nombre: Joi.string().min(1).required(),
  marca: Joi.string().min(1).required(),
  modelo: Joi.string().min(1).required(),
  precio: Joi.number().positive().required(),
  anio: Joi.number().integer().min(1900).max(2100).required(),
  categoria: Joi.string().min(1).required(),
});
