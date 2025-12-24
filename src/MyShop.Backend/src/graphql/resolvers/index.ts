import { DateTimeResolver } from 'graphql-scalars';
import { GraphQLScalarType, Kind } from 'graphql';
import { authResolvers } from './auth.resolver';
import { userResolvers } from './user.resolver';
import { categoryResolvers } from './category.resolver';
import { productResolvers } from './product.resolver';
import { customerResolvers } from './customer.resolver';
import { discountResolvers } from './discount.resolver';
import { orderResolvers } from './order.resolver';
import { dashboardResolvers } from './dashboard.resolver';
import { commissionResolvers } from './commission.resolver';
import { salesTargetResolvers } from './salesTarget.resolver';
import { appLicenseResolvers } from './appLicense.resolver';
import { reportResolvers } from './report.resolver';

// Custom Decimal scalar for handling numbers
const DecimalScalar = new GraphQLScalarType({
  name: 'Decimal',
  description: 'A Decimal number scalar',
  serialize(value: any) {
    // Convert to number for output
    return typeof value === 'object' && value !== null ? parseFloat(value.toString()) : parseFloat(value);
  },
  parseValue(value: any) {
    // Convert input to number
    return parseFloat(value);
  },
  parseLiteral(ast) {
    if (ast.kind === Kind.FLOAT || ast.kind === Kind.INT) {
      return parseFloat(ast.value);
    }
    return null;
  },
});

export const resolvers = {
  DateTime: DateTimeResolver,
  Decimal: DecimalScalar,

  Query: {
    ...authResolvers.Query,
    ...userResolvers.Query,
    ...categoryResolvers.Query,
    ...productResolvers.Query,
    ...customerResolvers.Query,
    ...discountResolvers.Query,
    ...orderResolvers.Query,
    ...dashboardResolvers.Query,
    ...reportResolvers.Query,
    ...commissionResolvers.Query,
    ...salesTargetResolvers.Query,
    ...appLicenseResolvers.Query,
  },

  Mutation: {
    ...authResolvers.Mutation,
    ...userResolvers.Mutation,
    ...categoryResolvers.Mutation,
    ...productResolvers.Mutation,
    ...customerResolvers.Mutation,
    ...discountResolvers.Mutation,
    ...orderResolvers.Mutation,
    ...commissionResolvers.Mutation,
    ...salesTargetResolvers.Mutation,
    ...appLicenseResolvers.Mutation,
  },

  // Field resolvers
  Category: categoryResolvers.Category,
  Product: productResolvers.Product,
  Customer: customerResolvers.Customer,
  Order: orderResolvers.Order,
  OrderItem: orderResolvers.OrderItem,
  Commission: commissionResolvers.Commission,
  SalesTarget: salesTargetResolvers.SalesTarget,
  AppLicense: appLicenseResolvers.AppLicense,
};
